-- Скрипт создания базы данных для модуля пользователей и центров
-- СУБД: PostgreSQL 15+

-- =====================================================
-- Таблица users (учётные записи)
-- =====================================================
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    role VARCHAR(50) NOT NULL, -- 'client', 'tutor', 'admin', 'super_admin'
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    confirmed_at TIMESTAMPTZ
);

COMMENT ON TABLE users IS 'Учётные записи пользователей';
COMMENT ON COLUMN users.role IS 'Роль в системе: client, tutor, admin, super_admin';

-- Индексы для users
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);

-- =====================================================
-- Таблица centers (репетиторские центры)
-- =====================================================
CREATE TABLE centers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    legal_name VARCHAR(255),
    inn VARCHAR(12) UNIQUE,
    address TEXT,
    phone VARCHAR(20),
    email VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_verified BOOLEAN NOT NULL DEFAULT FALSE
);

COMMENT ON TABLE centers IS 'Репетиторские центры';

-- Индексы для centers
CREATE INDEX idx_centers_name ON centers(name);
CREATE INDEX idx_centers_inn ON centers(inn) WHERE inn IS NOT NULL;

-- =====================================================
-- Таблица tutor_profiles (профили репетиторов)
-- =====================================================
CREATE TABLE tutor_profiles (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    biography TEXT,
    specialization VARCHAR(255),
    hourly_rate DECIMAL(10,2),
    education TEXT,
    experience_years VARCHAR(100),
    work_model VARCHAR(50) NOT NULL, -- 'individual', 'center'
    profile verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE tutor_profiles IS 'Профили репетиторов';
COMMENT ON COLUMN tutor_profiles.work_model IS 'Модель работы: individual, center';

-- Индексы для tutor_profiles
CREATE INDEX idx_tutor_profiles_user_id ON tutor_profiles(user_id);
CREATE INDEX idx_tutor_profiles_specialization ON tutor_profiles(specialization);
CREATE INDEX idx_tutor_profiles_work_model ON tutor_profiles(work_model);

-- =====================================================
-- Таблица client_profiles (профили клиентов)
-- =====================================================
CREATE TABLE client_profiles (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    birth_date DATE,
    is_adult BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE client_profiles IS 'Профили клиентов (родители или взрослые ученики)';

-- Индексы для client_profiles
CREATE INDEX idx_client_profiles_user_id ON client_profiles(user_id);

-- =====================================================
-- Таблица admin_profiles (профили администраторов центров)
-- =====================================================
CREATE TABLE admin_profiles (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    center_id INTEGER NOT NULL REFERENCES centers(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    job_title VARCHAR(255),
    permissions_level VARCHAR(50) NOT NULL, -- 'super_admin', 'center_admin', 'moderator'
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE admin_profiles IS 'Профили администраторов (привязаны к центру)';
COMMENT ON COLUMN admin_profiles.permissions_level IS 'Уровень прав: super_admin, center_admin, moderator';

-- Индексы для admin_profiles
CREATE INDEX idx_admin_profiles_user_id ON admin_profiles(user_id);
CREATE INDEX idx_admin_profiles_center_id ON admin_profiles(center_id);

-- =====================================================
-- Таблица students (ученики, привязанные к клиентам)
-- =====================================================
CREATE TABLE students (
    id SERIAL PRIMARY KEY,
    client_profile_id INTEGER NOT NULL REFERENCES client_profiles(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    grade INTEGER,
    birth_date DATE,
    is_self BOOLEAN NOT NULL DEFAULT FALSE,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE students IS 'Ученики (дети или сам клиент)';
COMMENT ON COLUMN students.is_self IS 'true если ученик — это сам клиент';

-- Индексы для students
CREATE INDEX idx_students_client_profile_id ON students(client_profile_id);
CREATE INDEX idx_students_full_name ON students(full_name);

-- =====================================================
-- Таблица center_membership (членство репетиторов в центрах)
-- =====================================================
CREATE TABLE center_membership (
    id SERIAL PRIMARY KEY,
    center_id INTEGER NOT NULL REFERENCES centers(id) ON DELETE CASCADE,
    tutor_profile_id INTEGER NOT NULL REFERENCES tutor_profiles(id) ON DELETE CASCADE,
    approved_by_admin_id INTEGER REFERENCES admin_profiles(id) ON DELETE SET NULL,
    joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    left_at TIMESTAMPTZ,
    join_method VARCHAR(50) NOT NULL, -- 'invitation', 'self_request', 'admin_added', 'by_link'
    status VARCHAR(50) NOT NULL DEFAULT 'active', -- 'active', 'left', 'suspended'
    leave_reason TEXT,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(center_id, tutor_profile_id, left_at) -- гарантия уникальности активной записи? обычно нужно, чтобы одновременно была только одна активная запись, но можно контролировать приложением
);

COMMENT ON TABLE center_membership IS 'Состав центра (история членства репетиторов)';
COMMENT ON COLUMN center_membership.status IS 'active, left, suspended';
COMMENT ON COLUMN center_membership.join_method IS 'Способ присоединения';

-- Индексы для center_membership
CREATE INDEX idx_center_membership_center_id ON center_membership(center_id);
CREATE INDEX idx_center_membership_tutor_profile_id ON center_membership(tutor_profile_id);
CREATE INDEX idx_center_membership_status ON center_membership(status);
CREATE INDEX idx_center_membership_joined_at ON center_membership(joined_at);

-- Чтобы гарантировать только одну активную запись для пары центр-репетитор,
-- можно создать частичный уникальный индекс:
CREATE UNIQUE INDEX idx_unique_active_membership 
ON center_membership(center_id, tutor_profile_id) 
WHERE status = 'active';

-- =====================================================
-- Добавление внешних ключей, которые ссылаются на созданные таблицы
-- (если необходимо связать что-то ещё)
-- =====================================================

-- Например, в tutor_profiles больше нет center_id, всё через membership

-- =====================================================
-- Комментарии к таблицам (опционально)
-- =====================================================

COMMENT ON TABLE users IS 'Пользователи системы (аутентификация)';
COMMENT ON TABLE tutor_profiles IS 'Данные репетиторов';
COMMENT ON TABLE client_profiles IS 'Данные клиентов (родители/взрослые ученики)';
COMMENT ON TABLE admin_profiles IS 'Администраторы центров';
COMMENT ON TABLE students IS 'Ученики, прикреплённые к клиентам';
COMMENT ON TABLE centers IS 'Репетиторские центры';
COMMENT ON TABLE center_membership IS 'Связь репетиторов с центрами (история)';