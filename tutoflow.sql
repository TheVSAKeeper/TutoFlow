-- Скрипт создания базы данных для модуля пользователей и центров
-- СУБД: PostgreSQL 15+

-- =====================================================
-- Перечисления (ENUM)
-- =====================================================
CREATE TYPE user_role AS ENUM ('client', 'tutor', 'admin', 'super_admin');
CREATE TYPE work_model AS ENUM ('individual', 'center');
CREATE TYPE permissions_level AS ENUM ('super_admin', 'center_admin', 'moderator');
CREATE TYPE join_method AS ENUM ('invitation', 'self_request', 'admin_added', 'by_link');
CREATE TYPE membership_status AS ENUM ('active', 'left', 'suspended');

-- =====================================================
-- Таблица users (учётные записи)
-- =====================================================
CREATE TABLE users (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    role user_role NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    confirmed_at TIMESTAMPTZ
);

COMMENT ON TABLE users IS 'Учётные записи пользователей';
COMMENT ON COLUMN users.role IS 'Роль в системе: client, tutor, admin, super_admin';

-- email уже имеет UNIQUE-индекс, дополнительный не нужен
CREATE INDEX idx_users_role ON users(role);

-- =====================================================
-- Таблица centers (репетиторские центры)
-- =====================================================
CREATE TABLE centers (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    legal_name VARCHAR(255),
    inn VARCHAR(12) UNIQUE,
    address TEXT,
    phone VARCHAR(20),
    email VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT chk_centers_inn CHECK (inn ~ '^\d{10,12}$')
);

COMMENT ON TABLE centers IS 'Репетиторские центры';

CREATE INDEX idx_centers_name ON centers(name);
CREATE INDEX idx_centers_inn ON centers(inn) WHERE inn IS NOT NULL;

-- =====================================================
-- Таблица tutor_profiles (профили репетиторов)
-- =====================================================
CREATE TABLE tutor_profiles (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    biography TEXT,
    specialization VARCHAR(255),
    hourly_rate DECIMAL(10,2),
    education TEXT,
    experience_years SMALLINT,
    work_model work_model NOT NULL,
    profile_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_tutor_hourly_rate CHECK (hourly_rate > 0),
    CONSTRAINT chk_tutor_experience CHECK (experience_years >= 0)
);

COMMENT ON TABLE tutor_profiles IS 'Профили репетиторов';
COMMENT ON COLUMN tutor_profiles.work_model IS 'Модель работы: individual, center';

-- user_id уже имеет UNIQUE-индекс, дополнительный не нужен
CREATE INDEX idx_tutor_profiles_specialization ON tutor_profiles(specialization);
CREATE INDEX idx_tutor_profiles_work_model ON tutor_profiles(work_model);

-- =====================================================
-- Таблица client_profiles (профили клиентов)
-- =====================================================
CREATE TABLE client_profiles (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    birth_date DATE,
    is_adult BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE client_profiles IS 'Профили клиентов (родители или взрослые ученики)';

-- user_id уже имеет UNIQUE-индекс, дополнительный не нужен

-- =====================================================
-- Таблица admin_profiles (профили администраторов центров)
-- =====================================================
CREATE TABLE admin_profiles (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    center_id INTEGER NOT NULL REFERENCES centers(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    job_title VARCHAR(255),
    permissions_level permissions_level NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE admin_profiles IS 'Профили администраторов (привязаны к центру)';
COMMENT ON COLUMN admin_profiles.permissions_level IS 'Уровень прав: super_admin, center_admin, moderator';

-- user_id уже имеет UNIQUE-индекс, дополнительный не нужен
CREATE INDEX idx_admin_profiles_center_id ON admin_profiles(center_id);

-- =====================================================
-- Таблица students (ученики, привязанные к клиентам)
-- =====================================================
CREATE TABLE students (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    client_profile_id INTEGER NOT NULL REFERENCES client_profiles(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    grade SMALLINT,
    birth_date DATE,
    is_self BOOLEAN NOT NULL DEFAULT FALSE,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_students_grade CHECK (grade BETWEEN 1 AND 12)
);

COMMENT ON TABLE students IS 'Ученики (дети или сам клиент)';
COMMENT ON COLUMN students.is_self IS 'true если ученик — это сам клиент';

CREATE INDEX idx_students_client_profile_id ON students(client_profile_id);
CREATE INDEX idx_students_full_name ON students(full_name);

-- =====================================================
-- Таблица center_membership (членство репетиторов в центрах)
-- =====================================================
CREATE TABLE center_membership (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    center_id INTEGER NOT NULL REFERENCES centers(id) ON DELETE CASCADE,
    tutor_profile_id INTEGER NOT NULL REFERENCES tutor_profiles(id) ON DELETE CASCADE,
    approved_by_admin_id INTEGER REFERENCES admin_profiles(id) ON DELETE SET NULL,
    joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    left_at TIMESTAMPTZ,
    join_method join_method NOT NULL,
    status membership_status NOT NULL DEFAULT 'active',
    leave_reason TEXT,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE center_membership IS 'Состав центра (история членства репетиторов)';
COMMENT ON COLUMN center_membership.status IS 'active, left, suspended';
COMMENT ON COLUMN center_membership.join_method IS 'Способ присоединения';

CREATE INDEX idx_center_membership_center_id ON center_membership(center_id);
CREATE INDEX idx_center_membership_tutor_profile_id ON center_membership(tutor_profile_id);
CREATE INDEX idx_center_membership_status ON center_membership(status);
CREATE INDEX idx_center_membership_joined_at ON center_membership(joined_at);

-- Гарантирует только одну активную запись для пары центр-репетитор
CREATE UNIQUE INDEX idx_unique_active_membership
ON center_membership(center_id, tutor_profile_id)
WHERE status = 'active';