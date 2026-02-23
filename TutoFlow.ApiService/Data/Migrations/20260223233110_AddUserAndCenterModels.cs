using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TutoFlow.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndCenterModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:join_method", "none,invitation,self_request,admin_added,by_link")
                .Annotation("Npgsql:Enum:membership_status", "none,active,left,suspended")
                .Annotation("Npgsql:Enum:permissions_level", "none,super_admin,center_admin,moderator")
                .Annotation("Npgsql:Enum:user_role", "none,client,tutor,admin,super_admin")
                .Annotation("Npgsql:Enum:work_model", "none,individual,center");

            migrationBuilder.CreateTable(
                name: "centers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    legal_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centers", x => x.id);
                    table.CheckConstraint("chk_centers_inn", "inn ~ '^\\d{10,12}$'");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    role = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admin_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    center_id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    job_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    permissions_level = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_admin_profiles_centers_center_id",
                        column: x => x.center_id,
                        principalTable: "centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_admin_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "client_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_adult = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_client_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tutor_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    biography = table.Column<string>(type: "text", nullable: true),
                    specialization = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    hourly_rate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    education = table.Column<string>(type: "text", nullable: true),
                    experience_years = table.Column<short>(type: "smallint", nullable: true),
                    work_model = table.Column<int>(type: "integer", nullable: false),
                    profile_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutor_profiles", x => x.id);
                    table.CheckConstraint("chk_tutor_experience", "experience_years >= 0");
                    table.CheckConstraint("chk_tutor_hourly_rate", "hourly_rate > 0");
                    table.ForeignKey(
                        name: "FK_tutor_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    client_profile_id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    grade = table.Column<short>(type: "smallint", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_self = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.id);
                    table.CheckConstraint("chk_students_grade", "grade BETWEEN 1 AND 12");
                    table.ForeignKey(
                        name: "FK_students_client_profiles_client_profile_id",
                        column: x => x.client_profile_id,
                        principalTable: "client_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "center_membership",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    center_id = table.Column<int>(type: "integer", nullable: false),
                    tutor_profile_id = table.Column<int>(type: "integer", nullable: false),
                    approved_by_admin_id = table.Column<int>(type: "integer", nullable: true),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    left_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    join_method = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    leave_reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_center_membership", x => x.id);
                    table.ForeignKey(
                        name: "FK_center_membership_admin_profiles_approved_by_admin_id",
                        column: x => x.approved_by_admin_id,
                        principalTable: "admin_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_center_membership_centers_center_id",
                        column: x => x.center_id,
                        principalTable: "centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_center_membership_tutor_profiles_tutor_profile_id",
                        column: x => x.tutor_profile_id,
                        principalTable: "tutor_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_admin_profiles_center_id",
                table: "admin_profiles",
                column: "center_id");

            migrationBuilder.CreateIndex(
                name: "IX_admin_profiles_user_id",
                table: "admin_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_center_membership_center_id",
                table: "center_membership",
                column: "center_id");

            migrationBuilder.CreateIndex(
                name: "idx_center_membership_joined_at",
                table: "center_membership",
                column: "joined_at");

            migrationBuilder.CreateIndex(
                name: "idx_center_membership_status",
                table: "center_membership",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_center_membership_tutor_profile_id",
                table: "center_membership",
                column: "tutor_profile_id");

            migrationBuilder.CreateIndex(
                name: "idx_unique_active_membership",
                table: "center_membership",
                columns: new[] { "center_id", "tutor_profile_id" },
                unique: true,
                filter: "status = 'active'");

            migrationBuilder.CreateIndex(
                name: "IX_center_membership_approved_by_admin_id",
                table: "center_membership",
                column: "approved_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "idx_centers_inn",
                table: "centers",
                column: "inn",
                unique: true,
                filter: "inn IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_centers_name",
                table: "centers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_client_profiles_user_id",
                table: "client_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_students_client_profile_id",
                table: "students",
                column: "client_profile_id");

            migrationBuilder.CreateIndex(
                name: "idx_students_full_name",
                table: "students",
                column: "full_name");

            migrationBuilder.CreateIndex(
                name: "idx_tutor_profiles_specialization",
                table: "tutor_profiles",
                column: "specialization");

            migrationBuilder.CreateIndex(
                name: "idx_tutor_profiles_work_model",
                table: "tutor_profiles",
                column: "work_model");

            migrationBuilder.CreateIndex(
                name: "IX_tutor_profiles_user_id",
                table: "tutor_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_role",
                table: "users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "center_membership");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "admin_profiles");

            migrationBuilder.DropTable(
                name: "tutor_profiles");

            migrationBuilder.DropTable(
                name: "client_profiles");

            migrationBuilder.DropTable(
                name: "centers");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:join_method", "none,invitation,self_request,admin_added,by_link")
                .OldAnnotation("Npgsql:Enum:membership_status", "none,active,left,suspended")
                .OldAnnotation("Npgsql:Enum:permissions_level", "none,super_admin,center_admin,moderator")
                .OldAnnotation("Npgsql:Enum:user_role", "none,client,tutor,admin,super_admin")
                .OldAnnotation("Npgsql:Enum:work_model", "none,individual,center");
        }
    }
}
