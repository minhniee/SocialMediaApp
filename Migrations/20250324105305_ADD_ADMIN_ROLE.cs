using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class ADD_ADMIN_ROLE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserViewModel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostCount = table.Column<int>(type: "int", nullable: false),
                    FriendCount = table.Column<int>(type: "int", nullable: false),
                    UserViewModelId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserViewModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserViewModel_UserViewModel_UserViewModelId",
                        column: x => x.UserViewModelId,
                        principalTable: "UserViewModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PostViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    IsLikedByCurrentUser = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostViewModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostViewModel_UserViewModel_UserId",
                        column: x => x.UserId,
                        principalTable: "UserViewModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CommentViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PostViewModelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentViewModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentViewModel_PostViewModel_PostViewModelId",
                        column: x => x.PostViewModelId,
                        principalTable: "PostViewModel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommentViewModel_UserViewModel_UserId",
                        column: x => x.UserId,
                        principalTable: "UserViewModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentViewModel_PostViewModelId",
                table: "CommentViewModel",
                column: "PostViewModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentViewModel_UserId",
                table: "CommentViewModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostViewModel_UserId",
                table: "PostViewModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserViewModel_UserViewModelId",
                table: "UserViewModel",
                column: "UserViewModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentViewModel");

            migrationBuilder.DropTable(
                name: "PostViewModel");

            migrationBuilder.DropTable(
                name: "UserViewModel");
        }
    }
}
