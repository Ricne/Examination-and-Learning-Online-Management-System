using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OMS.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    roleid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rolename = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.roleid);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    subjectid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subjectname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.subjectid);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fullname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    roleid = table.Column<int>(type: "integer", nullable: false),
                    avatarurl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userid);
                    table.ForeignKey(
                        name: "FK_users_roles_roleid",
                        column: x => x.roleid,
                        principalTable: "roles",
                        principalColumn: "roleid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    courseid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    coursename = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subjectid = table.Column<int>(type: "integer", nullable: false),
                    teacherid = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.courseid);
                    table.ForeignKey(
                        name: "FK_courses_subjects_subjectid",
                        column: x => x.subjectid,
                        principalTable: "subjects",
                        principalColumn: "subjectid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_courses_users_teacherid",
                        column: x => x.teacherid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refreshtokens",
                columns: table => new
                {
                    tokenid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    jwtid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revokedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replacedbytoken = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    createdbyip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    revokedbyip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    useragent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refreshtokens", x => x.tokenid);
                    table.ForeignKey(
                        name: "FK_refreshtokens_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coursestudents",
                columns: table => new
                {
                    courseid = table.Column<int>(type: "integer", nullable: false),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    enrolledat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coursestudents", x => new { x.courseid, x.studentid });
                    table.ForeignKey(
                        name: "FK_coursestudents_courses_courseid",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "courseid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_coursestudents_users_studentid",
                        column: x => x.studentid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    examid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    courseid = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    durationminutes = table.Column<int>(type: "integer", nullable: false),
                    starttime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    endtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    totalmarks = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    maxattempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    requireaccesscode = table.Column<bool>(type: "boolean", nullable: false),
                    allowreview = table.Column<bool>(type: "boolean", nullable: false),
                    reviewavailablefrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewavailableto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ispublished = table.Column<bool>(type: "boolean", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exams", x => x.examid);
                    table.ForeignKey(
                        name: "FK_exams_courses_courseid",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "courseid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exams_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    lessonid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    courseid = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    videourl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    attachmenturl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    lessonorder = table.Column<int>(type: "integer", nullable: false),
                    ispublished = table.Column<bool>(type: "boolean", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessons", x => x.lessonid);
                    table.ForeignKey(
                        name: "FK_lessons_courses_courseid",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "courseid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    questionid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    courseid = table.Column<int>(type: "integer", nullable: false),
                    questioncontent = table.Column<string>(type: "text", nullable: false),
                    questiontype = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    marks = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.questionid);
                    table.ForeignKey(
                        name: "FK_questions_courses_courseid",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "courseid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_questions_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examaccesscodes",
                columns: table => new
                {
                    codeid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    examid = table.Column<int>(type: "integer", nullable: false),
                    accesscode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    createdby = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    maxuses = table.Column<int>(type: "integer", nullable: true),
                    usedcount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examaccesscodes", x => x.codeid);
                    table.ForeignKey(
                        name: "FK_examaccesscodes_exams_examid",
                        column: x => x.examid,
                        principalTable: "exams",
                        principalColumn: "examid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_examaccesscodes_users_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examattempts",
                columns: table => new
                {
                    attemptid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    examid = table.Column<int>(type: "integer", nullable: false),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    starttime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    submittime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    totalscore = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "inprogress"),
                    usedaccesscode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examattempts", x => x.attemptid);
                    table.ForeignKey(
                        name: "FK_examattempts_exams_examid",
                        column: x => x.examid,
                        principalTable: "exams",
                        principalColumn: "examid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_examattempts_users_studentid",
                        column: x => x.studentid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessonprogress",
                columns: table => new
                {
                    lessonid = table.Column<int>(type: "integer", nullable: false),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    iscompleted = table.Column<bool>(type: "boolean", nullable: false),
                    completedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessonprogress", x => new { x.lessonid, x.studentid });
                    table.ForeignKey(
                        name: "FK_lessonprogress_lessons_lessonid",
                        column: x => x.lessonid,
                        principalTable: "lessons",
                        principalColumn: "lessonid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lessonprogress_users_studentid",
                        column: x => x.studentid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "choices",
                columns: table => new
                {
                    choiceid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    questionid = table.Column<int>(type: "integer", nullable: false),
                    choicetext = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    iscorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_choices", x => x.choiceid);
                    table.ForeignKey(
                        name: "fk_choices_question",
                        column: x => x.questionid,
                        principalTable: "questions",
                        principalColumn: "questionid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examquestion",
                columns: table => new
                {
                    examid = table.Column<int>(type: "integer", nullable: false),
                    questionid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examquestion", x => new { x.examid, x.questionid });
                    table.ForeignKey(
                        name: "FK_examquestion_exams_examid",
                        column: x => x.examid,
                        principalTable: "exams",
                        principalColumn: "examid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_examquestion_questions_questionid",
                        column: x => x.questionid,
                        principalTable: "questions",
                        principalColumn: "questionid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examresults",
                columns: table => new
                {
                    resultid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attemptid = table.Column<int>(type: "integer", nullable: false),
                    finalscore = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    gradedby = table.Column<int>(type: "integer", nullable: false),
                    gradedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examresults", x => x.resultid);
                    table.ForeignKey(
                        name: "FK_examresults_examattempts_attemptid",
                        column: x => x.attemptid,
                        principalTable: "examattempts",
                        principalColumn: "attemptid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_examresults_users_gradedby",
                        column: x => x.gradedby,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "studentanswers",
                columns: table => new
                {
                    answerid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attemptid = table.Column<int>(type: "integer", nullable: false),
                    questionid = table.Column<int>(type: "integer", nullable: false),
                    selectedchoiceid = table.Column<int>(type: "integer", nullable: true),
                    essayanswer = table.Column<string>(type: "text", nullable: true),
                    score = table.Column<decimal>(type: "numeric(7,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_studentanswers", x => x.answerid);
                    table.ForeignKey(
                        name: "FK_studentanswers_choices_selectedchoiceid",
                        column: x => x.selectedchoiceid,
                        principalTable: "choices",
                        principalColumn: "choiceid");
                    table.ForeignKey(
                        name: "FK_studentanswers_examattempts_attemptid",
                        column: x => x.attemptid,
                        principalTable: "examattempts",
                        principalColumn: "attemptid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_studentanswers_questions_questionid",
                        column: x => x.questionid,
                        principalTable: "questions",
                        principalColumn: "questionid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_choices_questionid",
                table: "choices",
                column: "questionid");

            migrationBuilder.CreateIndex(
                name: "ix_courses_isdeleted",
                table: "courses",
                column: "isdeleted");

            migrationBuilder.CreateIndex(
                name: "ix_courses_subjectid",
                table: "courses",
                column: "subjectid");

            migrationBuilder.CreateIndex(
                name: "ix_courses_teacherid",
                table: "courses",
                column: "teacherid");

            migrationBuilder.CreateIndex(
                name: "ix_cs_studentid",
                table: "coursestudents",
                column: "studentid");

            migrationBuilder.CreateIndex(
                name: "ix_eac_examid",
                table: "examaccesscodes",
                column: "examid");

            migrationBuilder.CreateIndex(
                name: "ix_eac_expiresat",
                table: "examaccesscodes",
                column: "expiresat");

            migrationBuilder.CreateIndex(
                name: "ix_eac_isactive",
                table: "examaccesscodes",
                column: "isactive");

            migrationBuilder.CreateIndex(
                name: "IX_examaccesscodes_createdby",
                table: "examaccesscodes",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "uq_eac_code",
                table: "examaccesscodes",
                column: "accesscode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attempt_examid",
                table: "examattempts",
                column: "examid");

            migrationBuilder.CreateIndex(
                name: "ix_attempt_status",
                table: "examattempts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_attempt_studentid",
                table: "examattempts",
                column: "studentid");

            migrationBuilder.CreateIndex(
                name: "IX_examquestion_questionid",
                table: "examquestion",
                column: "questionid");

            migrationBuilder.CreateIndex(
                name: "ix_examresults_gradedby",
                table: "examresults",
                column: "gradedby");

            migrationBuilder.CreateIndex(
                name: "UQ__examresu__93079C1F09A332B6",
                table: "examresults",
                column: "attemptid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exams_courseid",
                table: "exams",
                column: "courseid");

            migrationBuilder.CreateIndex(
                name: "IX_exams_createdby",
                table: "exams",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "ix_exams_isdeleted",
                table: "exams",
                column: "isdeleted");

            migrationBuilder.CreateIndex(
                name: "ix_exams_ispublished",
                table: "exams",
                column: "ispublished");

            migrationBuilder.CreateIndex(
                name: "ix_exams_requireaccesscode",
                table: "exams",
                column: "requireaccesscode");

            migrationBuilder.CreateIndex(
                name: "ix_lp_studentid",
                table: "lessonprogress",
                column: "studentid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_courseid",
                table: "lessons",
                column: "courseid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_isdeleted",
                table: "lessons",
                column: "isdeleted");

            migrationBuilder.CreateIndex(
                name: "uq_lesson_order",
                table: "lessons",
                columns: new[] { "courseid", "lessonorder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_questions_courseid",
                table: "questions",
                column: "courseid");

            migrationBuilder.CreateIndex(
                name: "IX_questions_createdby",
                table: "questions",
                column: "createdby");

            migrationBuilder.CreateIndex(
                name: "ix_questions_isdeleted",
                table: "questions",
                column: "isdeleted");

            migrationBuilder.CreateIndex(
                name: "ix_refreshtokens_expiresat",
                table: "refreshtokens",
                column: "expiresat");

            migrationBuilder.CreateIndex(
                name: "ix_refreshtokens_userid",
                table: "refreshtokens",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "ux_refreshtokens_token",
                table: "refreshtokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__roles__4685A0620D799878",
                table: "roles",
                column: "rolename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_studentanswers_attemptid",
                table: "studentanswers",
                column: "attemptid");

            migrationBuilder.CreateIndex(
                name: "ix_studentanswers_questionid",
                table: "studentanswers",
                column: "questionid");

            migrationBuilder.CreateIndex(
                name: "IX_studentanswers_selectedchoiceid",
                table: "studentanswers",
                column: "selectedchoiceid");

            migrationBuilder.CreateIndex(
                name: "ux_studentanswers_attempt_question",
                table: "studentanswers",
                columns: new[] { "attemptid", "questionid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subjects_isdeleted",
                table: "subjects",
                column: "isdeleted");

            migrationBuilder.CreateIndex(
                name: "uq_subjects_name",
                table: "subjects",
                column: "subjectname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_roleid",
                table: "users",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "uq_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coursestudents");

            migrationBuilder.DropTable(
                name: "examaccesscodes");

            migrationBuilder.DropTable(
                name: "examquestion");

            migrationBuilder.DropTable(
                name: "examresults");

            migrationBuilder.DropTable(
                name: "lessonprogress");

            migrationBuilder.DropTable(
                name: "refreshtokens");

            migrationBuilder.DropTable(
                name: "studentanswers");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "choices");

            migrationBuilder.DropTable(
                name: "examattempts");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
