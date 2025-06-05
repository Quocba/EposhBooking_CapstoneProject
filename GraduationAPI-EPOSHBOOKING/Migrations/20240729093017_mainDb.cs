using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationAPI_EPOSHBOOKING.Migrations
{
    /// <inheritdoc />
    public partial class mainDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HotelAddress",
                columns: table => new
                {
                    AddressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    latitude = table.Column<double>(type: "float", nullable: false),
                    longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelAddress", x => x.AddressID);
                });

            migrationBuilder.CreateTable(
                name: "Profile",
                columns: table => new
                {
                    ProfileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fullName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BirthDay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profile", x => x.ProfileID);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Voucher",
                columns: table => new
                {
                    VoucherID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    QuantityUse = table.Column<int>(type: "int", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voucher", x => x.VoucherID);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    AccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    ProfileID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_Account_Profile_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "Profile",
                        principalColumn: "ProfileID");
                    table.ForeignKey(
                        name: "FK_Account_Role_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Role",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    BlogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonReject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.BlogID);
                    table.ForeignKey(
                        name: "FK_Blogs_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "AccountID");
                });

            migrationBuilder.CreateTable(
                name: "Hotel",
                columns: table => new
                {
                    HotelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MainImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenedIn = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HotelStandar = table.Column<int>(type: "int", nullable: false),
                    isRegister = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    AddressID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotel", x => x.HotelID);
                    table.ForeignKey(
                        name: "FK_Hotel_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hotel_HotelAddress_AddressID",
                        column: x => x.AddressID,
                        principalTable: "HotelAddress",
                        principalColumn: "AddressID");
                });

            migrationBuilder.CreateTable(
                name: "MyVoucher",
                columns: table => new
                {
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    VoucherID = table.Column<int>(type: "int", nullable: false),
                    IsVoucher = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyVoucher", x => new { x.VoucherID, x.AccountID });
                    table.ForeignKey(
                        name: "FK_MyVoucher_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MyVoucher_Voucher_VoucherID",
                        column: x => x.VoucherID,
                        principalTable: "Voucher",
                        principalColumn: "VoucherID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogImage",
                columns: table => new
                {
                    ImageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlogID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogImage", x => x.ImageID);
                    table.ForeignKey(
                        name: "FK_BlogImage_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentBlog",
                columns: table => new
                {
                    CommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateComment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BlogID = table.Column<int>(type: "int", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentBlog", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_CommentBlog_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentBlog_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelImage",
                columns: table => new
                {
                    ImageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HotelID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelImage", x => x.ImageID);
                    table.ForeignKey(
                        name: "FK_HotelImage_Hotel_HotelID",
                        column: x => x.HotelID,
                        principalTable: "Hotel",
                        principalColumn: "HotelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelService",
                columns: table => new
                {
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HotelID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelService", x => x.ServiceID);
                    table.ForeignKey(
                        name: "FK_HotelService_Hotel_HotelID",
                        column: x => x.HotelID,
                        principalTable: "Hotel",
                        principalColumn: "HotelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeOfRoom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberCapacity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SizeOfRoom = table.Column<int>(type: "int", nullable: false),
                    TypeOfBed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfBed = table.Column<int>(type: "int", nullable: false),
                    HotelID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.RoomID);
                    table.ForeignKey(
                        name: "FK_Room_Hotel_HotelID",
                        column: x => x.HotelID,
                        principalTable: "Hotel",
                        principalColumn: "HotelID");
                });

            migrationBuilder.CreateTable(
                name: "HotelSubService",
                columns: table => new
                {
                    SubServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelSubService", x => x.SubServiceID);
                    table.ForeignKey(
                        name: "FK_HotelSubService_HotelService_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "HotelService",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    BookingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPrice = table.Column<double>(type: "float", nullable: false),
                    UnitPrice = table.Column<double>(type: "float", nullable: false),
                    TaxesPrice = table.Column<double>(type: "float", nullable: false),
                    NumberOfRoom = table.Column<int>(type: "int", nullable: false),
                    NumberGuest = table.Column<int>(type: "int", nullable: false),
                    ReasonCancle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountID = table.Column<int>(type: "int", nullable: true),
                    RoomID = table.Column<int>(type: "int", nullable: true),
                    VoucherID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.BookingID);
                    table.ForeignKey(
                        name: "FK_Booking_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "FK_Booking_Room_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Room",
                        principalColumn: "RoomID");
                    table.ForeignKey(
                        name: "FK_Booking_Voucher_VoucherID",
                        column: x => x.VoucherID,
                        principalTable: "Voucher",
                        principalColumn: "VoucherID");
                });

            migrationBuilder.CreateTable(
                name: "RoomImage",
                columns: table => new
                {
                    ImageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomImage", x => x.ImageID);
                    table.ForeignKey(
                        name: "FK_RoomImage_Room_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Room",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomService",
                columns: table => new
                {
                    RoomServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomService", x => x.RoomServiceID);
                    table.ForeignKey(
                        name: "FK_RoomService_Room_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Room",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialPrice",
                columns: table => new
                {
                    SpecialPriceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialPrice", x => x.SpecialPriceID);
                    table.ForeignKey(
                        name: "FK_SpecialPrice_Room_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Room",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedBack",
                columns: table => new
                {
                    FeedBackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountID = table.Column<int>(type: "int", nullable: true),
                    BookingID = table.Column<int>(type: "int", nullable: true),
                    HotelID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedBack", x => x.FeedBackID);
                    table.ForeignKey(
                        name: "FK_FeedBack_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "FK_FeedBack_Booking_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Booking",
                        principalColumn: "BookingID");
                    table.ForeignKey(
                        name: "FK_FeedBack_Hotel_HotelID",
                        column: x => x.HotelID,
                        principalTable: "Hotel",
                        principalColumn: "HotelID");
                });

            migrationBuilder.CreateTable(
                name: "RoomSubService",
                columns: table => new
                {
                    SubServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomServiceID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomSubService", x => x.SubServiceID);
                    table.ForeignKey(
                        name: "FK_RoomSubService_RoomService_RoomServiceID",
                        column: x => x.RoomServiceID,
                        principalTable: "RoomService",
                        principalColumn: "RoomServiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportFeedBack",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReporterEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonReport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeedBackID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFeedBack", x => x.ReportID);
                    table.ForeignKey(
                        name: "FK_ReportFeedBack_FeedBack_FeedBackID",
                        column: x => x.FeedBackID,
                        principalTable: "FeedBack",
                        principalColumn: "FeedBackID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_ProfileID",
                table: "Account",
                column: "ProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Account_RoleID",
                table: "Account",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_BlogImage_BlogID",
                table: "BlogImage",
                column: "BlogID");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_AccountID",
                table: "Blogs",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_AccountID",
                table: "Booking",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_RoomID",
                table: "Booking",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_VoucherID",
                table: "Booking",
                column: "VoucherID");

            migrationBuilder.CreateIndex(
                name: "IX_CommentBlog_AccountID",
                table: "CommentBlog",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_CommentBlog_BlogID",
                table: "CommentBlog",
                column: "BlogID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBack_AccountID",
                table: "FeedBack",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBack_BookingID",
                table: "FeedBack",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBack_HotelID",
                table: "FeedBack",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_Hotel_AccountID",
                table: "Hotel",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Hotel_AddressID",
                table: "Hotel",
                column: "AddressID");

            migrationBuilder.CreateIndex(
                name: "IX_HotelImage_HotelID",
                table: "HotelImage",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_HotelService_HotelID",
                table: "HotelService",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_HotelSubService_ServiceID",
                table: "HotelSubService",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_MyVoucher_AccountID",
                table: "MyVoucher",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFeedBack_FeedBackID",
                table: "ReportFeedBack",
                column: "FeedBackID");

            migrationBuilder.CreateIndex(
                name: "IX_Room_HotelID",
                table: "Room",
                column: "HotelID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomImage_RoomID",
                table: "RoomImage",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomService_RoomID",
                table: "RoomService",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomSubService_RoomServiceID",
                table: "RoomSubService",
                column: "RoomServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialPrice_RoomID",
                table: "SpecialPrice",
                column: "RoomID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogImage");

            migrationBuilder.DropTable(
                name: "CommentBlog");

            migrationBuilder.DropTable(
                name: "HotelImage");

            migrationBuilder.DropTable(
                name: "HotelSubService");

            migrationBuilder.DropTable(
                name: "MyVoucher");

            migrationBuilder.DropTable(
                name: "ReportFeedBack");

            migrationBuilder.DropTable(
                name: "RoomImage");

            migrationBuilder.DropTable(
                name: "RoomSubService");

            migrationBuilder.DropTable(
                name: "SpecialPrice");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "HotelService");

            migrationBuilder.DropTable(
                name: "FeedBack");

            migrationBuilder.DropTable(
                name: "RoomService");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Voucher");

            migrationBuilder.DropTable(
                name: "Hotel");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "HotelAddress");

            migrationBuilder.DropTable(
                name: "Profile");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
