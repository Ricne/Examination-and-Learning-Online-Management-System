using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OMS.Migrations
{
    /// <inheritdoc />
    public partial class InitSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    roleid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    rolename = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__roles__CD994BF2D0E014A6", x => x.roleid);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    subjectid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    subjectname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    isactive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    isdeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    deletedat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    createdat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__subjects__ACE1437884B53055", x => x.subjectid);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    passwordhash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    fullname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    roleid = table.Column<int>(type: "INTEGER", nullable: false),
                    avatarurl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    isactive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    createdat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__CBA1B25799860EF1", x => x.userid);
                    table.ForeignKey(
                        name: "fk_users_role",
                        column: x => x.roleid,
                        principalTable: "roles",
                        principalColumn: "roleid");
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    courseid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    coursename = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    subjectid = table.Column<int>(type: "INTEGER", nullable: false),
                    teacherid = table.Column<int>(type: "INTEGER", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    isactive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    isdeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    deletedat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    createdat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__courses__2AAB4BC9471B9734", x => x.courseid);
                    table.ForeignKey(
                        name: "fk_courses_subject",
                        column: x => x.subjectid,
                        principalTable: "subjects",
                        principalColumn: "subjectid");
                    table.ForeignKey(
                        name: "fk_courses_teacher",
                        column: x => x.teacherid,
                        principalTable: "users",
                        principalColumn: "userid");
                });

            migrationBuilder.CreateTable(
                name: "refreshtokens",
                columns: table => new
                {
                    tokenid = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userid = table.Column<int>(type: "INTEGER", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    jwtid = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    createdat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "TEXT", nullable: false),
                    revokedat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    replacedbytoken = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    createdbyip = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    revokedbyip = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    useragent = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__refresht__AC17DF2F2CE6675E", x => x.tokenid);
                    table.ForeignKey(
                        name: "fk_refreshtokens_user",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coursestudents",
                columns: table => new
                {
                    courseid = table.Column<int>(type: "INTEGER", nullable: false),
                    studentid = table.Column<int>(type: "INTEGER", nullable: false),
                    enrolledat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__coursest__7E7A26EFA1FDA1A7", x => new { x.courseid, x.studentid });
                    table.ForeignKey(
                        name: "fk_cs_course",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "courseid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cs_student",
                        column: x => x.studentid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    examid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    courseid = table.Column<int>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    durationminutes = table.Column<int>(type: "INTEGER", nullable: false),
                    starttime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    endtime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    totalmarks = table.Column<decimal>(type: "decimal(7, 2)", nullable: false),
                    maxattempts = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    requireaccesscode = table.Column<bool>(type: "INTEGER", nullable: false),
                    allowreview = table.Column<bool>(type: "INTEGER", nullable: false),
                    reviewavailablefrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    reviewavailableto = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ispublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    isdeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    deletedat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    createdby = table.Column<int>(type: "INTEGER", nullable: false),
                    createdat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__exams__A56C2E67A94BE1C6", x => x.examid);
                    table.ForeignKey(
                        name: "fk_exams_course",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "courseid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exams_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid");
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    lessonid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    courseid = table.Column<int>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "TEXT", nullable: true),
                    videourl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    attachmenturl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    lessonorder = table.Column<int>(type: "INTEGER", nullable: false),
                    ispublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    isdeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    deletedat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    createdat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__lessons__F88B935051A31C27", x => x.lessonid);
                    table.ForeignKey(
                        name: "fk_lessons_course",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "courseid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    questionid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    courseid = table.Column<int>(type: "INTEGER", nullable: false),
                    questioncontent = table.Column<string>(type: "TEXT", nullable: false),
                    questiontype = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    marks = table.Column<decimal>(type: "decimal(7, 2)", nullable: false),
                    createdby = table.Column<int>(type: "INTEGER", nullable: false),
                    isdeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    deletedat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    createdat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__question__62C2216AEFD90A2D", x => x.questionid);
                    table.ForeignKey(
                        name: "fk_questions_course",
                        column: x => x.courseid,
                        principalTable: "courses",
                        principalColumn: "courseid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_questions_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid");
                });

            migrationBuilder.CreateTable(
                name: "examaccesscodes",
                columns: table => new
                {
                    codeid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    examid = table.Column<int>(type: "INTEGER", nullable: false),
                    accesscode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    createdby = table.Column<int>(type: "INTEGER", nullable: false),
                    createdat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    isactive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    maxuses = table.Column<int>(type: "INTEGER", nullable: true),
                    usedcount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__examacce__47F9C38CABBE8D99", x => x.codeid);
                    table.ForeignKey(
                        name: "fk_eac_createdby",
                        column: x => x.createdby,
                        principalTable: "users",
                        principalColumn: "userid");
                    table.ForeignKey(
                        name: "fk_eac_exam",
                        column: x => x.examid,
                        principalTable: "exams",
                        principalColumn: "examid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examattempts",
                columns: table => new
                {
                    attemptid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    examid = table.Column<int>(type: "INTEGER", nullable: false),
                    studentid = table.Column<int>(type: "INTEGER", nullable: false),
                    starttime = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    submittime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    totalscore = table.Column<decimal>(type: "decimal(7, 2)", nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "inprogress"),
                    usedaccesscode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__examatte__93079C1E5CEF0DC2", x => x.attemptid);
                    table.ForeignKey(
                        name: "fk_attempt_exam",
                        column: x => x.examid,
                        principalTable: "exams",
                        principalColumn: "examid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attempt_student",
                        column: x => x.studentid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessonprogress",
                columns: table => new
                {
                    lessonid = table.Column<int>(type: "INTEGER", nullable: false),
                    studentid = table.Column<int>(type: "INTEGER", nullable: false),
                    iscompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    completedat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__lessonpr__AC5AFE76518F04CF", x => new { x.lessonid, x.studentid });
                    table.ForeignKey(
                        name: "fk_lp_lesson",
                        column: x => x.lessonid,
                        principalTable: "lessons",
                        principalColumn: "lessonid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lp_student",
                        column: x => x.studentid,
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "choices",
                columns: table => new
                {
                    choiceid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    questionid = table.Column<int>(type: "INTEGER", nullable: false),
                    choicetext = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    iscorrect = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__choices__16C8B75F6987F2BB", x => x.choiceid);
                    table.ForeignKey(
                        name: "fk_choices_question",
                        column: x => x.questionid,
                        principalTable: "questions",
                        principalColumn: "questionid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examquestions",
                columns: table => new
                {
                    examid = table.Column<int>(type: "INTEGER", nullable: false),
                    questionid = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__examques__13400C718E31CCD8", x => new { x.examid, x.questionid });
                    table.ForeignKey(
                        name: "fk_eq_exam",
                        column: x => x.examid,
                        principalTable: "exams",
                        principalColumn: "examid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_eq_question",
                        column: x => x.questionid,
                        principalTable: "questions",
                        principalColumn: "questionid");
                });

            migrationBuilder.CreateTable(
                name: "examresults",
                columns: table => new
                {
                    resultid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    attemptid = table.Column<int>(type: "INTEGER", nullable: false),
                    finalscore = table.Column<decimal>(type: "decimal(7, 2)", nullable: false),
                    gradedby = table.Column<int>(type: "INTEGER", nullable: false),
                    gradedat = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__examresu__C6EBD0433EC3794D", x => x.resultid);
                    table.ForeignKey(
                        name: "fk_result_attempt",
                        column: x => x.attemptid,
                        principalTable: "examattempts",
                        principalColumn: "attemptid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_result_gradedby",
                        column: x => x.gradedby,
                        principalTable: "users",
                        principalColumn: "userid");
                });

            migrationBuilder.CreateTable(
                name: "studentanswers",
                columns: table => new
                {
                    answerid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    attemptid = table.Column<int>(type: "INTEGER", nullable: false),
                    questionid = table.Column<int>(type: "INTEGER", nullable: false),
                    selectedchoiceid = table.Column<int>(type: "INTEGER", nullable: true),
                    essayanswer = table.Column<string>(type: "TEXT", nullable: true),
                    score = table.Column<decimal>(type: "decimal(7, 2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__studenta__6837BD9C4E00477D", x => x.answerid);
                    table.ForeignKey(
                        name: "fk_sa_attempt",
                        column: x => x.attemptid,
                        principalTable: "examattempts",
                        principalColumn: "attemptid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sa_choice",
                        column: x => x.selectedchoiceid,
                        principalTable: "choices",
                        principalColumn: "choiceid");
                    table.ForeignKey(
                        name: "fk_sa_question",
                        column: x => x.questionid,
                        principalTable: "questions",
                        principalColumn: "questionid");
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
                name: "ix_examquestions_questionid",
                table: "examquestions",
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
                name: "examquestions");

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
                name: "examattempts");

            migrationBuilder.DropTable(
                name: "choices");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "questions");

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
