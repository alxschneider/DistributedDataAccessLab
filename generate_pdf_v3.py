"""Generate PDF for V3 / V3 Plus deliverable - Alexandre Schneider, CSCI 6844 V1"""

from fpdf import FPDF

OUTPUT_PDF = "Alexandre_schneider_6844_v3.pdf"

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
            self.cell(0, 8, "DistributedDataAccessLab - V3 Third Deliverable", align="C")
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
        self.cell(0, 10, "V3 - Third Deliverable", align="C", new_x="LMARGIN", new_y="NEXT")
        self.ln(4)
        self.set_font(FONT, "", 13)
        self.set_text_color(*GRAY_TEXT)
        self.cell(0, 8, "E-Commerce Marketplace - Blazor Frontends", align="C", new_x="LMARGIN", new_y="NEXT")
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


def build_pdf():
    pdf = PDF("P", "mm", "Letter")
    pdf.set_auto_page_break(auto=True, margin=20)
    pdf.set_left_margin(20)
    pdf.set_right_margin(20)
    pdf.setup_fonts()

    pdf.cover_page()
    pdf.add_page()

    usable = pdf.w - pdf.l_margin - pdf.r_margin

    # --- Overview ---
    pdf.section_title("V3 - Third Deliverable Overview")
    pdf.body_text(
        "This deliverable adds two Blazor Server frontends to the existing microservices backend. "
        "Both frontends run as Docker containers alongside the 6 backend services, RabbitMQ, and the API Gateway, "
        "bringing the total to 10 containers managed by docker-compose."
    )

    pdf.subsection_title("Complete Service Map (10 containers)")
    pdf.table(
        ["Service", "Port", "Description"],
        [
            ["API Gateway (YARP)", ":5000", "Reverse proxy routing + aggregated endpoint"],
            ["CustomerService", ":5001", "Buyers/sellers management, role-based dashboards"],
            ["OrderService", ":5002", "Order creation, status transitions, RabbitMQ publisher"],
            ["ProductService", ":5003", "Product catalog, stock, discounts"],
            ["CartService", ":5005", "Shopping cart with checkout orchestration"],
            ["PaymentService", ":5006", "Payment processing (mock 90% approval)"],
            ["NotificationService", ":5007", "Notifications + RabbitMQ consumers"],
            ["RabbitMQ", ":5672/:15672", "Message broker (management UI on 15672)"],
            ["SimpleFrontend", ":5010", "Simple Blazor dashboard with data tables"],
            ["WebFrontend", ":5020", "AI-generated FDU-themed e-commerce marketplace"],
        ],
        [usable * 0.25, usable * 0.15, usable * 0.60],
    )

    # === A) SimpleFrontend ===
    pdf.section_title("A) SimpleFrontend (port 5010)")
    pdf.bold_text("Objective: Minimal Blazor Server frontend using the default project template with Bootstrap tables.")
    pdf.body_text(
        "A straightforward dashboard that calls the API Gateway and displays raw data from every microservice. "
        "No custom styling - uses the default Blazor template with standard Bootstrap classes. "
        "The goal is to demonstrate the frontend consuming all backend endpoints via IHttpClientFactory."
    )

    pdf.subsection_title("Project Structure")
    pdf.table(
        ["File", "Description"],
        [
            ["SimpleFrontend.csproj", ".NET 10 Blazor Server project (default template)"],
            ["Program.cs", "Registers IHttpClientFactory with named 'Api' client pointing to API Gateway"],
            ["Dockerfile", "Multi-stage build (sdk:10.0-alpine -> aspnet:10.0-alpine)"],
            ["Components/Layout/NavMenu.razor", "Navigation with 7 links: Home, Customers, Products, Orders, Cart, Payments, Notifications"],
        ],
        [usable * 0.40, usable * 0.60],
    )

    pdf.subsection_title("Blazor Pages")
    pdf.table(
        ["Page", "Route", "Functionality"],
        [
            ["Home.razor", "/", "Dashboard: service health table with OK/Error badges and record counts"],
            ["Customers.razor", "/customers", "Auto-loaded table: ID, Name, Email, Phone, Role (badge), Address, Active, Created"],
            ["Products.razor", "/products", "Auto-loaded table: ID, Seller, Name, Category, Price, Stock, Discount%, Final Price"],
            ["Orders.razor", "/orders", "Order table + Create Order form (Buyer ID, Product ID, Qty) - triggers RabbitMQ event"],
            ["CartPage.razor", "/cart", "Lookup by Buyer ID - cart items table + Add Item form"],
            ["Payments.razor", "/payments", "Auto-loaded table: ID, Order, Buyer, Amount, Method, Status (color badge), timestamps"],
            ["Notifications.razor", "/notifications", "Lookup by User ID - notifications table showing RabbitMQ-generated entries"],
        ],
        [usable * 0.20, usable * 0.15, usable * 0.65],
    )

    pdf.subsection_title("Key Technical Details")
    pdf.bullet("Uses IHttpClientFactory with named client 'Api' - base URL configurable via Services__ApiGateway env var")
    pdf.bullet("All pages use @rendermode InteractiveServer for real-time interactivity")
    pdf.bullet("Create Order form on Orders page publishes OrderCreatedEvent via RabbitMQ (through the backend)")
    pdf.bullet("Notification page shows entries created by RabbitMQ consumers in real-time")
    pdf.bullet("Docker: runs on port 5010, depends_on apigateway")

    # === B) WebFrontend ===
    pdf.section_title("B) WebFrontend (port 5020) - AI-Generated FDU Course Marketplace")
    pdf.bold_text("Objective: Fully styled Blazor e-commerce frontend built with AI-assisted web scraping and code generation.")

    pdf.subsection_title("How it was built")
    pdf.body_text(
        "1. Web Scraping: The FDU website (fdu.edu) was scraped to capture the university's visual identity - "
        "colors, typography, layout patterns, logo usage, and overall design language."
    )
    pdf.body_text(
        "2. AI-Generated CSS/HTML: Using the scraped reference material, AI (GitHub Copilot) generated all the "
        "custom CSS, HTML structure, component layouts, and responsive design from scratch. No FDU assets, images, "
        "or copyrighted content were copied; only the visual style was used as inspiration."
    )
    pdf.body_text(
        "3. E-Commerce Course Marketplace: The concept is a course-selling platform where FDU sellers list "
        "courses/products and buyers can browse, add to cart, place orders, and receive notifications."
    )

    pdf.subsection_title("Project Structure")
    pdf.table(
        ["File/Folder", "Description"],
        [
            ["WebFrontend.csproj", ".NET 10 Blazor Server project"],
            ["Program.cs", "6 typed HttpClient services (one per microservice, direct calls) + ProfileState"],
            ["Models/", "Customer.cs, Product.cs, Order.cs, Cart.cs, Payment.cs, Notification.cs"],
            ["Services/", "CustomerService.cs, ProductService.cs, OrderService.cs, CartService.cs, PaymentService.cs, NotificationService.cs, ProfileState.cs"],
            ["wwwroot/app.css", "FDU-inspired custom CSS: maroon/burgundy primary, gold accents, dark tones"],
            ["Components/Layout/", "MainLayout.razor (custom styled), NavMenu.razor (role-based navigation)"],
            ["Dockerfile", "Multi-stage build (same pattern as other services)"],
        ],
        [usable * 0.30, usable * 0.70],
    )

    pdf.subsection_title("Pages")
    pdf.table(
        ["Page", "Route", "Functionality"],
        [
            ["Home.razor", "/", "Landing page with FDU-themed hero section and marketplace overview"],
            ["Dashboard.razor", "/dashboard", "Role-based dashboard (Admin/Buyer/Seller) with stats and quick actions"],
            ["Customers.razor", "/customers", "Customer management with role badges and detail views"],
            ["Products.razor", "/products", "Product catalog with seller info, discounts, stock management"],
            ["Orders.razor", "/orders", "Order management with status transitions and cancel/return flows"],
            ["CartPage.razor", "/cart", "Shopping cart with checkout orchestration"],
            ["Payments.razor", "/payments", "Payment history with status badges and processing"],
            ["Notifications.razor", "/notifications", "Notification center with read/unread management"],
        ],
        [usable * 0.20, usable * 0.15, usable * 0.65],
    )

    pdf.subsection_title("Key Features")
    pdf.bullet("FDU-inspired color scheme: CSS variables with maroon/burgundy primary, gold accents, dark backgrounds")
    pdf.bullet("Role-based ProfileState: Admin sees everything, Buyer sees own orders/cart, Seller sees own products/sales")
    pdf.bullet("Typed HttpClient services per microservice (CustomerService, ProductService, etc.) - NO API Gateway dependency")
    pdf.bullet("Full CRUD operations on all pages with forms and validation")
    pdf.bullet("Responsive layout with Blazor CSS isolation (.razor.css files)")
    pdf.bullet("Docker: runs on port 5020, env vars configure direct service URLs")

    # === C) Docker Compose ===
    pdf.section_title("C) Docker Compose Updates")
    pdf.bold_text("Two new services added to docker-compose.yml:")

    pdf.table(
        ["Service", "Configuration"],
        [
            ["simplefrontend", "Build: SimpleFrontend/SimpleFrontend, Port: 5010->8080, env: Services__ApiGateway=http://apigateway:8080, depends_on: apigateway"],
            ["webfrontend", "Build: WebFrontend/WebFrontend, Port: 5020->8080, env: 6 service URLs (direct), depends_on: apigateway"],
        ],
        [usable * 0.20, usable * 0.80],
    )

    pdf.body_text("Total containers: 10 (6 microservices + RabbitMQ + API Gateway + SimpleFrontend + WebFrontend)")

    # === D) Other Changes ===
    pdf.section_title("D) Other Changes")
    pdf.table(
        ["File", "Change"],
        [
            [".gitignore", "Removed 'WebFrontend/' exclusion to include it in the repository"],
            ["DistributedDataAccessLab.sln", "Added SimpleFrontend project (WebFrontend was already in solution)"],
            ["README.md", "Updated service table (10 services) + added V3 Plus section with AI explanation"],
        ],
        [usable * 0.35, usable * 0.65],
    )

    # === Architecture ===
    pdf.add_page()
    pdf.section_title("Final Architecture (V3)")
    pdf.code_block(
        "   SimpleFrontend :5010          WebFrontend :5020\n"
        "   (default Blazor)              (FDU-themed AI)\n"
        "        |                              |\n"
        "        | (via API Gateway)            | (direct to services)\n"
        "        v                              v\n"
        "  +-------------------+     +---------+---------+\n"
        "  |   API Gateway     |     |                   |\n"
        "  |   (YARP) :5000    |     |                   |\n"
        "  +--------+----------+     |                   |\n"
        "           |                |                   |\n"
        "   +-------+------+---------+---+-------+-------+\n"
        "   |       |      |         |   |       |       |\n"
        " Customer Prod  Order     Cart Pay   Notif\n"
        "  :5001  :5003  :5002    :5005 :5006  :5007\n"
        "                  |                     |\n"
        "                  |  +--------------+   |\n"
        "                  +->|  RabbitMQ    |<--+\n"
        "                     | :5672/:15672 |\n"
        "                     +--------------+"
    )

    pdf.subsection_title("Frontend Comparison")
    pdf.table(
        ["Aspect", "SimpleFrontend", "WebFrontend"],
        [
            ["Port", "5010", "5020"],
            ["Styling", "Default Blazor template + Bootstrap", "Custom AI-generated FDU theme"],
            ["API Communication", "Via API Gateway (IHttpClientFactory)", "Direct to each microservice (typed HttpClients)"],
            ["Pages", "7 pages with data tables", "8+ pages with full CRUD forms"],
            ["Role-based UI", "No", "Yes (Admin/Buyer/Seller profiles)"],
            ["CSS", "Standard Bootstrap only", "Custom CSS variables, Blazor CSS isolation"],
            ["Purpose", "Simple functional demo", "Production-like AI-generated marketplace"],
            ["Built with AI?", "Code-assisted", "Full scraping + AI CSS/HTML generation"],
        ],
        [usable * 0.20, usable * 0.40, usable * 0.40],
    )

    pdf.subsection_title("AI as a Development Accelerator")
    pdf.body_text(
        "The WebFrontend demonstrates how AI can be used as a development tool. The entire workflow was: "
        "(1) scrape the FDU website for design reference, (2) feed the visual patterns to GitHub Copilot, "
        "(3) generate all CSS variables, component layouts, and responsive breakpoints, (4) integrate with "
        "the microservices backend. No copyrighted assets were used - only the visual style as inspiration."
    )

    pdf.output(OUTPUT_PDF)
    print(f"PDF generated: {OUTPUT_PDF}")


if __name__ == "__main__":
    build_pdf()
