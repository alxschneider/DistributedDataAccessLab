"""Generate PDF from CHANGELOG_V2.md - Alexandre Schneider, CSCI 6844 V1"""

from fpdf import FPDF

OUTPUT_PDF = "Alexandre_schneider_6844_v2.pdf"

# Colors
DARK_BLUE = (26, 58, 92)
MED_BLUE = (44, 95, 138)
WHITE = (255, 255, 255)
LIGHT_GRAY = (245, 247, 250)
GRAY_TEXT = (100, 100, 100)
BLACK = (34, 34, 34)
CODE_BG = (240, 240, 240)
TH_BG = (26, 58, 92)

FONT = "Ari"
MONO = "Mon"


class PDF(FPDF):
    def setup_fonts(self):
        self.add_font(FONT, "", "C:/Windows/Fonts/arial.ttf", uni=True)
        self.add_font(FONT, "B", "C:/Windows/Fonts/arialbd.ttf", uni=True)
        self.add_font(FONT, "I", "C:/Windows/Fonts/ariali.ttf", uni=True)
        self.add_font(MONO, "", "C:/Windows/Fonts/consola.ttf", uni=True)

    def header(self):
        if self.page_no() > 1:
            self.set_font(FONT, "I", 8)
            self.set_text_color(*GRAY_TEXT)
            self.cell(0, 8, "DistributedDataAccessLab - V2 Second Deliverable", align="C")
            self.ln(4)

    def footer(self):
        if self.page_no() > 1:
            self.set_y(-15)
            self.set_font(FONT, "I", 8)
            self.set_text_color(*GRAY_TEXT)
            self.cell(0, 10, str(self.page_no() - 1), align="C")

    def cover_page(self):
        self.add_page()
        self.ln(80)
        self.set_font(FONT, "B", 28)
        self.set_text_color(*DARK_BLUE)
        self.cell(0, 14, "DistributedDataAccessLab", align="C", new_x="LMARGIN", new_y="NEXT")
        self.ln(8)
        self.set_font(FONT, "B", 16)
        self.cell(0, 10, "V2 - Second Deliverable", align="C", new_x="LMARGIN", new_y="NEXT")
        self.ln(4)
        self.set_font(FONT, "", 13)
        self.set_text_color(*GRAY_TEXT)
        self.cell(0, 8, "E-Commerce Marketplace Backend", align="C", new_x="LMARGIN", new_y="NEXT")
        self.ln(16)
        self.set_draw_color(200, 200, 200)
        x = self.w / 2 - 40
        self.line(x, self.get_y(), x + 80, self.get_y())
        self.ln(16)
        self.set_font(FONT, "B", 14)
        self.set_text_color(*BLACK)
        self.cell(0, 8, "Alexandre Schneider", align="C", new_x="LMARGIN", new_y="NEXT")
        self.ln(4)
        self.set_font(FONT, "", 12)
        self.set_text_color(*GRAY_TEXT)
        self.cell(0, 7, "FDU MSACS - CSCI 6844 V1", align="C", new_x="LMARGIN", new_y="NEXT")
        self.cell(0, 7, "Programming for the Internet", align="C", new_x="LMARGIN", new_y="NEXT")
        self.ln(6)
        self.cell(0, 7, "April 2026", align="C", new_x="LMARGIN", new_y="NEXT")

    def section_title(self, text):
        self.ln(6)
        self.set_font(FONT, "B", 15)
        self.set_text_color(*DARK_BLUE)
        self.cell(0, 10, text, new_x="LMARGIN", new_y="NEXT")
        self.set_draw_color(200, 200, 200)
        self.line(self.l_margin, self.get_y(), self.w - self.r_margin, self.get_y())
        self.ln(3)

    def subsection_title(self, text):
        self.ln(4)
        self.set_font(FONT, "B", 12)
        self.set_text_color(*MED_BLUE)
        self.cell(0, 8, text, new_x="LMARGIN", new_y="NEXT")
        self.ln(1)

    def body_text(self, text):
        self.set_font(FONT, "", 10)
        self.set_text_color(*BLACK)
        self.multi_cell(0, 5.5, text)
        self.ln(1)

    def bold_text(self, text):
        self.set_font(FONT, "B", 10)
        self.set_text_color(*BLACK)
        self.multi_cell(0, 5.5, text)
        self.ln(1)

    def bullet(self, text):
        self.set_font(FONT, "", 10)
        self.set_text_color(*BLACK)
        self.cell(6, 5.5, "-")
        self.multi_cell(0, 5.5, text)
        self.ln(0.5)

    def code_block(self, text):
        self.set_fill_color(*CODE_BG)
        self.set_font(MONO, "", 8.5)
        self.set_text_color(60, 60, 60)
        for line in text.strip().split("\n"):
            self.cell(0, 4.5, "  " + line, fill=True, new_x="LMARGIN", new_y="NEXT")
        self.ln(3)

    def table(self, headers, rows, col_widths=None):
        if col_widths is None:
            usable = self.w - self.l_margin - self.r_margin
            col_widths = [usable / len(headers)] * len(headers)

        self.set_font(FONT, "B", 8.5)
        self.set_fill_color(*TH_BG)
        self.set_text_color(*WHITE)
        for i, h in enumerate(headers):
            self.cell(col_widths[i], 7, " " + h, border=1, fill=True)
        self.ln()

        self.set_font(FONT, "", 8)
        for row_idx, row in enumerate(rows):
            self.set_text_color(*BLACK)
            fill_color = LIGHT_GRAY if row_idx % 2 == 1 else WHITE
            self.set_fill_color(*fill_color)

            max_lines = 1
            for i, cell_text in enumerate(row):
                lines = self.multi_cell(col_widths[i], 4.5, " " + cell_text, border=0, split_only=True)
                max_lines = max(max_lines, len(lines))

            row_h = max_lines * 4.5
            if self.get_y() + row_h > self.h - self.b_margin - 10:
                self.add_page()
                self.set_font(FONT, "B", 8.5)
                self.set_fill_color(*TH_BG)
                self.set_text_color(*WHITE)
                for i, h in enumerate(headers):
                    self.cell(col_widths[i], 7, " " + h, border=1, fill=True)
                self.ln()
                self.set_font(FONT, "", 8)
                self.set_fill_color(*fill_color)

            x_start = self.get_x()
            y_start = self.get_y()
            for i, cell_text in enumerate(row):
                self.set_text_color(*BLACK)
                self.set_xy(x_start + sum(col_widths[:i]), y_start)
                self.multi_cell(col_widths[i], 4.5, " " + cell_text, border=1, fill=True)
            self.set_xy(x_start, y_start + row_h)

        self.ln(3)


def parse_and_build():
    pdf = PDF("P", "mm", "Letter")
    pdf.set_auto_page_break(auto=True, margin=20)
    pdf.set_left_margin(20)
    pdf.set_right_margin(20)
    pdf.setup_fonts()

    pdf.cover_page()
    pdf.add_page()

    usable = pdf.w - pdf.l_margin - pdf.r_margin

    # --- Title & Meta ---
    pdf.section_title("V2 - Second Deliverable Changelog")
    pdf.bold_text("Commit: 5a6bdbb - V2: RabbitMQ messaging, API Gateway (YARP), DTOs, docker-compose update")
    pdf.body_text("Date: April 6, 2026")
    pdf.body_text("Stats: 33 files changed, +1,251 lines, -341 lines")

    # === A) RabbitMQ ===
    pdf.section_title("A) Asynchronous Messaging with RabbitMQ (MassTransit)")
    pdf.bold_text("Objective: Asynchronous communication between microservices using a message broker.")

    pdf.subsection_title("New files")
    pdf.table(
        ["File", "Description"],
        [
            ["OrderService/.../Contracts/Events.cs", "Event contracts (OrderCreatedEvent, OrderCancelledEvent, OrderItemEvent)"],
            ["NotificationService/.../Contracts/Events.cs", "Same contracts replicated (shared namespace Contracts.Events)"],
            ["NotificationService/.../Consumers/OrderCreatedConsumer.cs", "Consumer: receives OrderCreatedEvent, creates notifications for buyer & sellers"],
            ["NotificationService/.../Consumers/OrderCancelledConsumer.cs", "Consumer: receives OrderCancelledEvent, notifies buyer & sellers"],
        ],
        [usable * 0.45, usable * 0.55],
    )

    pdf.subsection_title("Modified files")
    pdf.table(
        ["File", "Change"],
        [
            ["OrderService/.../Program.cs", "Added AddMassTransit() with RabbitMQ config"],
            ["OrderService/.../OrderService.Api.csproj", "Added MassTransit.RabbitMQ 8.4.0 package"],
            ["OrderService/.../OrdersController.cs", "Injected IPublishEndpoint; Create publishes OrderCreatedEvent; Cancel publishes OrderCancelledEvent"],
            ["NotificationService/.../Program.cs", "Added AddMassTransit() with AddConsumer<OrderCreatedConsumer> and AddConsumer<OrderCancelledConsumer>"],
            ["NotificationService/.../.csproj", "Added MassTransit.RabbitMQ 8.4.0 package"],
        ],
        [usable * 0.40, usable * 0.60],
    )

    pdf.subsection_title("Event Flow")
    pdf.bullet("Buyer creates an order via POST /api/orders")
    pdf.bullet("OrdersController saves the order and publishes OrderCreatedEvent to RabbitMQ")
    pdf.bullet("OrderCreatedConsumer in NotificationService receives the event and creates notifications for buyer and each seller")
    pdf.bullet("Same flow for cancellation: OrderCancelledEvent -> OrderCancelledConsumer")

    # === B) API Gateway ===
    pdf.section_title("B) API Gateway with YARP")
    pdf.bold_text("Objective: Single entry point for all microservices + data aggregation endpoint.")

    pdf.subsection_title("New project: ApiGateway")
    pdf.table(
        ["File", "Description"],
        [
            ["ApiGateway.csproj", ".NET 10 project - Yarp.ReverseProxy 2.3.0, Swashbuckle 10.1.2"],
            ["Program.cs", "YARP reverse proxy setup + named HttpClients for aggregation + Swagger"],
            ["appsettings.json", "YARP routes for 6 microservices + clusters with Docker hostnames"],
            ["Controllers/AggregateController.cs", "GET /api/aggregate/order-details/{orderId} - calls Order, Customer, Product in parallel"],
            ["Dockerfile", "Multi-stage build following existing services pattern"],
        ],
        [usable * 0.35, usable * 0.65],
    )

    pdf.subsection_title("YARP Routes")
    pdf.table(
        ["Route", "Destination"],
        [
            ["/api/customers/{**catch-all}", "http://customerservice:8080"],
            ["/api/products/{**catch-all}", "http://productservice:8080"],
            ["/api/orders/{**catch-all}", "http://orderservice:8080"],
            ["/api/notifications/{**catch-all}", "http://notificationservice:8080"],
            ["/api/cart/{**catch-all}", "http://cartservice:8080"],
            ["/api/payments/{**catch-all}", "http://paymentservice:8080"],
        ],
        [usable * 0.45, usable * 0.55],
    )

    pdf.subsection_title("Aggregated Endpoint")
    pdf.body_text("GET /api/aggregate/order-details/{orderId} returns combined JSON with { order, customer, products } from 3 services in parallel.")

    # === C) DTOs ===
    pdf.section_title("C) DTOs in All Services")
    pdf.bold_text("Objective: Never expose domain entities directly in APIs; use request/response DTO records.")

    pdf.subsection_title("New DTO files (Dtos/ folder in each service)")
    pdf.table(
        ["Service", "DTOs Created"],
        [
            ["CustomerService", "CreateCustomerDto, UpdateCustomerDto, CustomerResponseDto, CustomerDetailDto, CustomerDashboardDto, PersonalDashboardDto, CustomerSummaryDto"],
            ["OrderService", "CreateOrderRequestDto, CreateOrderItemDto, UpdateStatusDto, CancelOrderDto, ReturnOrderDto, OrderResponseDto, OrderItemResponseDto, OrderDashboardDto, StatusCountDto, BuyerDashboardDto, LastOrderDto, SellerDashboardDto"],
            ["ProductService", "CreateProductDto, UpdateProductDto, StockUpdateDto, StockDecrementDto, DiscountUpdateDto, ProductResponseDto, ProductStockDto, ProductDiscountDto, ProductDashboardDto, CategoryStatsDto, SellerDashboardDto, SellerProductDto"],
            ["CartService", "AddCartItemDto, UpdateCartItemDto, CartResponseDto, CartItemResponseDto, EmptyCartDto, CheckoutResponseDto"],
            ["PaymentService", "CreatePaymentDto, PaymentResponseDto, PaymentProcessResultDto, PaymentDashboardDto, PaymentStatusStatsDto, PaymentMethodStatsDto"],
            ["NotificationService", "CreateNotificationDto, NotificationResponseDto, NotificationPageDto, UnreadCountDto, NotificationDashboardDto, TypeCountDto, UserUnreadDto, MarkReadResultDto"],
        ],
        [usable * 0.22, usable * 0.78],
    )

    pdf.subsection_title("Controllers modified (all 6)")
    pdf.table(
        ["Controller", "Changes"],
        [
            ["CustomersController.cs", "All endpoints return DTOs; Create/Update accept DTOs; added MapToDto()"],
            ["OrdersController.cs", "Same + removed old inline classes; deleted Models/Dtos.cs"],
            ["ProductsController.cs", "Same + removed inline StockUpdateRequest, StockDecrementRequest, DiscountUpdateRequest"],
            ["CartController.cs", "Same + removed inline request classes"],
            ["PaymentsController.cs", "Same + removed inline CreatePaymentRequest"],
            ["NotificationsController.cs", "Same pattern applied"],
        ],
        [usable * 0.30, usable * 0.70],
    )

    # === D) Docker Compose ===
    pdf.section_title("D) Updated docker-compose.yml")

    pdf.subsection_title("Services added")
    pdf.table(
        ["Service", "Config"],
        [
            ["rabbitmq", "Image: rabbitmq:3-management-alpine, Ports: 5672/15672, Healthcheck: rabbitmq-diagnostics -q ping"],
            ["apigateway", "Build: ApiGateway/ApiGateway, Port: 5000->8080, depends_on all 6 microservices, YARP env overrides"],
        ],
        [usable * 0.25, usable * 0.75],
    )

    pdf.subsection_title("Services modified")
    pdf.table(
        ["Service", "Change"],
        [
            ["orderservice", "Added RabbitMQ__Host: rabbitmq + depends_on rabbitmq (service_healthy)"],
            ["notificationservice", "Added RabbitMQ__Host: rabbitmq + depends_on rabbitmq (service_healthy)"],
        ],
        [usable * 0.25, usable * 0.75],
    )

    # === E) Other ===
    pdf.section_title("E) Other Changes")
    pdf.table(
        ["File", "Change"],
        [
            ["DistributedDataAccessLab.sln", "Added ApiGateway project to solution"],
            [".gitignore", "Added rules for *.db, *.db-shm, *.db-wal"],
            ["CustomerService/.../Dockerfile", "Minor build adjustment"],
            ["README.md", "Rewritten with service table, V2 section, updated tech stack"],
        ],
        [usable * 0.35, usable * 0.65],
    )

    # === Architecture ===
    pdf.add_page()
    pdf.section_title("Final Architecture (V2)")
    pdf.code_block(
        "                    +-------------------+\n"
        "                    |   API Gateway     | :5000\n"
        "                    |   (YARP Proxy)    |\n"
        "                    +--------+----------+\n"
        "                             |\n"
        "     +----------+--------+---+---+--------+---------+\n"
        "     |          |        |       |        |         |\n"
        " Customer  Product   Order    Cart   Payment  Notification\n"
        "  :5001     :5003    :5002   :5005   :5006     :5007\n"
        "                       |                         |\n"
        "                       |    +--------------+     |\n"
        "                       +--->|  RabbitMQ    |<----+\n"
        "                            | :5672/:15672 |\n"
        "                            +--------------+\n"
        "                       publish           consume\n"
        "                  OrderCreatedEvent  OrderCreatedConsumer\n"
        "                  OrderCancelledEvent OrderCancelledConsumer"
    )

    pdf.output(OUTPUT_PDF)
    print(f"PDF generated: {OUTPUT_PDF}")


if __name__ == "__main__":
    parse_and_build()
