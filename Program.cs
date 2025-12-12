using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace JB_POS_System
{
    #region


    class Product
    {
        public int Id;
        public string Name;
        public string Brand;
        public string Category;
        public int Stock;
        public double Price;
        public string Barcode;

        public Product() { }

        public Product(int id, string name, string brand, string category, int stock, double price, string barcode = "")
        {
            Id = id; Name = name; Brand = brand; Category = category; Stock = stock; Price = price; Barcode = barcode;
        }

        public virtual string ToFileLine() => $"{Id}|{Escape(Name)}|{Escape(Brand)}|{Escape(Category)}|{Stock}|{Price.ToString(CultureInfo.InvariantCulture)}|{Escape(Barcode)}";
        static string Escape(string s) => (s ?? "").Replace("|", " ");


        public static Product FromLine(string line)
        {
            var parts = line.Split('|');
            if (parts.Length < 6) return null;
            if (!int.TryParse(parts[0], out var id)) return null;
            var name = parts[1];
            var brand = parts[2];
            var category = parts[3];
            if (!int.TryParse(parts[4], out var stock)) stock = 0;
            if (!double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) price = 0;
            var barcode = parts.Length >= 7 ? parts[6] : "";


            if (category.Equals("Sneakers", StringComparison.OrdinalIgnoreCase))
                return new SneakerProduct(id, name, brand, category, stock, price, barcode);
            else if (category.Equals("Watches", StringComparison.OrdinalIgnoreCase))
                return new WatchProduct(id, name, brand, category, stock, price, barcode);
            else if (category.Equals("Streetwear", StringComparison.OrdinalIgnoreCase))
                return new StreetwearProduct(id, name, brand, category, stock, price, barcode);
            else
                return new Product(id, name, brand, category, stock, price, barcode);
        }

        public override string ToString() => $"{Id} | {Name} | {Brand} | {Category} | {Stock} | ₱{Price.ToString("N2", CultureInfo.InvariantCulture)} | {Barcode}";
    }

    class SneakerProduct : Product
    {
        public SneakerProduct(int id, string name, string brand, string category, int stock, double price, string barcode = "")
            : base(id, name, brand, category, stock, price, barcode) { }

    }

    class WatchProduct : Product
    {
        public WatchProduct(int id, string name, string brand, string category, int stock, double price, string barcode = "")
            : base(id, name, brand, category, stock, price, barcode) { }

    }

    class StreetwearProduct : Product
    {
        public StreetwearProduct(int id, string name, string brand, string category, int stock, double price, string barcode = "")
            : base(id, name, brand, category, stock, price, barcode) { }

    }

    class User
    {
        public string Username;
        public string Password;
        public User() { }
        public User(string u, string p) { Username = u; Password = p; }
        public virtual string ToFileLine() => $"{Username}|{Password}";
        public static User FromLine(string line)
        {
            var p = line.Split('|');
            if (p.Length < 2) return null;
            return new AdminUser(p[0], p[1]);
        }
    }

    class AdminUser : User
    {
        public AdminUser() { }
        public AdminUser(string u, string p) : base(u, p) { }
        public override string ToFileLine() => base.ToFileLine();
    }

    class Customer
    {
        public int Id;
        public string Name;
        public string Contact;
        public Customer() { }
        public Customer(int id, string name, string contact) { Id = id; Name = name; Contact = contact; }
        public override string ToString() => $"{Id}|{Escape(Name)}|{Escape(Contact)}";
        static string Escape(string s) => (s ?? "").Replace("|", " ");
        public static Customer FromLine(string line)
        {
            var p = line.Split('|');
            if (p.Length < 3) return null;
            if (!int.TryParse(p[0], out var id)) return null;
            return new Customer(id, p[1], p[2]);
        }
    }

    class Supplier
    {
        public int Id;
        public string Name;
        public string Contact;
        public Supplier() { }
        public Supplier(int id, string name, string contact) { Id = id; Name = name; Contact = contact; }
        public override string ToString() => $"{Id}|{Escape(Name)}|{Escape(Contact)}";
        static string Escape(string s) => (s ?? "").Replace("|", " ");
        public static Supplier FromLine(string line)
        {
            var p = line.Split('|');
            if (p.Length < 3) return null;
            if (!int.TryParse(p[0], out var id)) return null;
            return new Supplier(id, p[1], p[2]);
        }
    }

    class InvoiceItem
    {
        public int ProductId;
        public string ProductName;
        public int Quantity;
        public double UnitPrice;
        public double Total => UnitPrice * Quantity;

        public InvoiceItem() { }
        public InvoiceItem(int pid, string pname, int qty, double price) { ProductId = pid; ProductName = pname; Quantity = qty; UnitPrice = price; }

        public override string ToString() => $"{ProductId},{Escape(ProductName)},{Quantity},{UnitPrice.ToString(CultureInfo.InvariantCulture)}";
        static string Escape(string s) => (s ?? "").Replace(",", " ");
        public static InvoiceItem FromPart(string part)
        {
            var p = part.Split(',');
            if (p.Length < 4) return null;
            if (!int.TryParse(p[0], out var pid)) return null;
            if (!int.TryParse(p[2], out var qty)) return null;
            if (!double.TryParse(p[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) price = 0;
            return new InvoiceItem(pid, p[1], qty, price);
        }
    }

    class Invoice
    {
        public int Id;
        public string Type; 
        public string Party; 
        public DateTime Date;
        public List<InvoiceItem> Items = new List<InvoiceItem>();

        public Invoice() { }
        public Invoice(int id, string type, string party) { Id = id; Type = type; Party = party; Date = DateTime.Now; Items = new List<InvoiceItem>(); }

        public double GrandTotal => Items.Sum(i => i.Total);

        public override string ToString()
        {
            var itemsStr = string.Join(";", Items.Select(i => i.ToString()));
            return $"{Id}|{Type}|{Escape(Party)}|{Date.ToString("o")}|{itemsStr}";
        }
        static string Escape(string s) => (s ?? "").Replace("|", " ");

        public static Invoice FromLine(string line)
        {
            var p = line.Split('|');
            if (p.Length < 5) return null;
            if (!int.TryParse(p[0], out var id)) return null;
            var type = p[1];
            var party = p[2];
            if (!DateTime.TryParse(p[3], out var date)) date = DateTime.Now;
            var inv = new Invoice(id, type, party) { Date = date };
            var itemsRaw = p[4];
            if (!string.IsNullOrWhiteSpace(itemsRaw))
            {
                var parts = itemsRaw.Split(';');
                foreach (var part in parts)
                {
                    var it = InvoiceItem.FromPart(part);
                    if (it != null) inv.Items.Add(it);
                }
            }
            return inv;
        }
    }

    #endregion

    class Program
    {
        // Files & folders
        const string DataFolder = "data";
        const string AdminsFile = "data/admins.txt";
        const string ProductsFile = "products.txt";
        const string CustomersFile = "customers.txt";
        const string SuppliersFile = "suppliers.txt";
        const string SalesFile = "sales.txt";
        const string PurchasesFile = "purchases.txt";
        const string CategoriesFile = "categories.txt";
        const string InvoicesFolder = "Invoices";
        const string InventoryReceiptsFolder = "InventoryReceipts";

        // Data stores
        static List<Product> products = new List<Product>();
        static List<User> users = new List<User>();
        static List<Customer> customers = new List<Customer>();
        static List<Supplier> suppliers = new List<Supplier>();
        static List<Invoice> sales = new List<Invoice>();
        static List<Invoice> purchases = new List<Invoice>();

        // IDs
        static int nextProductId = 1;
        static int nextCustomerId = 1;
        static int nextSupplierId = 1;
        static int nextSaleId = 1;
        static int nextPurchaseId = 1;

        // UI
        static ConsoleColor TitleBorderColor = ConsoleColor.Cyan;
        static ConsoleColor SelectedBg = ConsoleColor.White;
        static ConsoleColor SelectedFg = ConsoleColor.Black;

        // Current logged admin (for receipts/invoices)
        static string CurrentAdmin = "";

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "JB SNEAKERS & APPAREL";

            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(InvoicesFolder);
            Directory.CreateDirectory(InventoryReceiptsFolder);

            EnsureFilesAndLoad();

            // If no admins yet, require creating the first admin
            if (!users.Any())
            {
                Console.Clear();
                WriteBoxedTitle("JB SNEAKERS & APPAREL", "NO ADMIN FOUND - CREATE ADMIN");
                Console.WriteLine("Create the first admin account (saved to data/admins.txt).");
                Register();
            }

            // login / register loop
            while (true)
            {
                Console.Clear();
                int lr = LoginRegisterMenu();
                if (lr == 0) // login
                {
                    var logged = Login();
                    if (logged != null)
                    {
                        CurrentAdmin = logged.Username;
                        ShowLowStockAlert();
                        MainMenu(logged);
                        CurrentAdmin = ""; // clear after logout
                    }
                }
                else if (lr == 1) // register
                {
                    Register();
                }
                else // exit
                {
                    return;
                }
            }
        }

        #region Ensure/Load/Save
        static void EnsureFilesAndLoad()
        {
            if (!File.Exists(AdminsFile)) File.WriteAllText(AdminsFile, "");
            users = File.ReadAllLines(AdminsFile).Where(l => !string.IsNullOrWhiteSpace(l)).Select(User.FromLine).Where(u => u != null).ToList();

            if (!File.Exists(CategoriesFile))
            {
                File.WriteAllLines(CategoriesFile, new[] { "Sneakers", "Watches", "Streetwear" });
            }

            if (!File.Exists(ProductsFile))
            {
                var sample = new List<string>
                {
                    new Product(1, "Air Runner", "Nike", "Sneakers", 10, 4999.50, "BAR0001").ToFileLine(),
                    new Product(2, "Street Classic", "Adidas", "Sneakers", 4, 3999.00, "BAR0002").ToFileLine(),
                    new Product(3, "Wrist Pro", "Casio", "Watches", 6, 2599.75, "BAR0003").ToFileLine()
                };
                File.WriteAllLines(ProductsFile, sample);
            }
            products = File.ReadAllLines(ProductsFile).Where(l => !string.IsNullOrWhiteSpace(l)).Select(Product.FromLine).Where(p => p != null).ToList();
            nextProductId = products.Any() ? products.Max(p => p.Id) + 1 : 1;

            if (!File.Exists(CustomersFile)) File.WriteAllText(CustomersFile, "");
            customers = File.ReadAllLines(CustomersFile).Where(l => !string.IsNullOrWhiteSpace(l)).Select(Customer.FromLine).Where(c => c != null).ToList();
            nextCustomerId = customers.Any() ? customers.Max(c => c.Id) + 1 : 1;

            if (!File.Exists(SuppliersFile)) File.WriteAllText(SuppliersFile, "");
            suppliers = File.ReadAllLines(SuppliersFile).Where(l => !string.IsNullOrWhiteSpace(l)).Select(Supplier.FromLine).Where(s => s != null).ToList();
            nextSupplierId = suppliers.Any() ? suppliers.Max(s => s.Id) + 1 : 1;

            if (!File.Exists(SalesFile)) File.WriteAllText(SalesFile, "");
            sales = File.ReadAllLines(SalesFile).Where(l => !string.IsNullOrWhiteSpace(l)).Select(Invoice.FromLine).Where(i => i != null && i.Type == "Sale").ToList();
            nextSaleId = sales.Any() ? sales.Max(i => i.Id) + 1 : 1;

            if (!File.Exists(PurchasesFile)) File.WriteAllText(PurchasesFile, "");
            purchases = File.ReadAllLines(PurchasesFile).Where(l => !string.IsNullOrWhiteSpace(l)).Select(Invoice.FromLine).Where(i => i != null && i.Type == "Purchase").ToList();
            nextPurchaseId = purchases.Any() ? purchases.Max(i => i.Id) + 1 : 1;
        }

        static void SaveProducts() { try { File.WriteAllLines(ProductsFile, products.Select(p => p.ToFileLine())); } catch { } }
        static void SaveAdmins() { try { File.WriteAllLines(AdminsFile, users.Select(u => u.ToFileLine())); } catch { } }
        static void SaveCustomers() { try { File.WriteAllLines(CustomersFile, customers.Select(c => c.ToString())); } catch { } }
        static void SaveSuppliers() { try { File.WriteAllLines(SuppliersFile, suppliers.Select(s => s.ToString())); } catch { } }
        static void SaveSales() { try { File.WriteAllLines(SalesFile, sales.Select(s => s.ToString())); } catch { } }
        static void SavePurchases() { try { File.WriteAllLines(PurchasesFile, purchases.Select(p => p.ToString())); } catch { } }

        static void SaveCategories(List<string> cats)
        {
            try
            {
                File.WriteAllLines(CategoriesFile, cats);
            }
            catch { }
        }
        #endregion

        #region Auth / Login / Register
        static int LoginRegisterMenu()
        {
            var options = new List<string> { "Login", "Register (create admin)", "Exit" };
            return MenuSelect(options, "LOGIN / REGISTER");
        }

        static AdminUser Login()
        {
            Console.Clear();
            WriteBoxedTitle("JB SNEAKERS & APPAREL", "LOGIN");
            Console.Write("Username: ");
            var u = Console.ReadLine()?.Trim();
            Console.Write("Password: ");
            var p = ReadPasswordMasked();
            var user = users.FirstOrDefault(x => x.Username.Equals(u, StringComparison.OrdinalIgnoreCase) && x.Password == p);
            if (user != null)
            {
                Console.WriteLine("\nLogin successful. Press any key...");
                Console.ReadKey();
                if (user is AdminUser a) return a;
                return new AdminUser(user.Username, user.Password);
            }
            Console.WriteLine("\nLogin failed. Press any key...");
            Console.ReadKey();
            return null;
        }

        static void Register()
        {
            Console.Clear();
            WriteBoxedTitle("JB SNEAKERS & APPAREL", "REGISTER (CREATE ADMIN)");
            Console.Write("New username: ");
            var u = Console.ReadLine()?.Trim();
            Console.Write("New password: ");
            var p = ReadPasswordMasked();
            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                Console.WriteLine("\nInvalid input. Press any key...");
                Console.ReadKey();
                return;
            }
            if (users.Any(x => x.Username.Equals(u, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("\nUsername exists. Press any key...");
                Console.ReadKey();
                return;
            }
            var admin = new AdminUser(u, p);
            users.Add(admin);
            SaveAdmins();
            Console.WriteLine("\nRegistered. You can now login. Press any key...");
            Console.ReadKey();
        }

        static string ReadPasswordMasked()
        {
            var pwd = new StringBuilder();
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.Length--;
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    pwd.Append(key.KeyChar);
                    Console.Write("•");
                }
            } while (true);
            Console.WriteLine();
            return pwd.ToString();
        }
        #endregion

        #region Menu system (arrow-key everywhere)
        static int MenuSelect(List<string> options, string title, string username = "")
        {
            int index = 0;
            ConsoleKeyInfo keyInfo;
            do
            {
                Console.Clear();
                string subtitle = string.IsNullOrWhiteSpace(username) ? title : $"{title} - Welcome, {username}";
                WriteBoxedTitle("JB SNEAKERS & APPAREL", subtitle);
                Console.WriteLine();
                for (int i = 0; i < options.Count; i++)
                {
                    if (i == index)
                    {
                        Console.BackgroundColor = SelectedBg;
                        Console.ForegroundColor = SelectedFg;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else Console.WriteLine($"  {options[i]}");
                }
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.UpArrow) { index--; if (index < 0) index = options.Count - 1; }
                else if (keyInfo.Key == ConsoleKey.DownArrow) { index++; if (index >= options.Count) index = 0; }
                else if (keyInfo.Key == ConsoleKey.Enter) { return index; }
                else if (keyInfo.Key == ConsoleKey.Escape) { return -1; }
            } while (true);
        }
        #endregion

        #region Main Menu & Submenus
        static void MainMenu(AdminUser admin)
        {
            var options = new List<string> { "Products", "Customers", "Suppliers", "Sales", "Purchases", "Reports", "Low Stock", "Logout", "Exit" };
            while (true)
            {
                int sel = MenuSelect(options, "MAIN MENU", admin.Username);
                if (sel == -1) continue;
                switch (sel)
                {
                    case 0: ProductsMenu(); break;
                    case 1: CustomersMenu(); break;
                    case 2: SuppliersMenu(); break;
                    case 3: SalesMenu(admin); break;
                    case 4: PurchasesMenu(admin); break;
                    case 5: ReportsMenu(); break;
                    case 6: ViewLowStockMenu(); break;
                    case 7: return;
                    case 8: Environment.Exit(0); break;
                }
            }
        }
        #endregion

        #region Products Menu & Actions (use CategorySelectArrow)
        static void ProductsMenu()
        {
            var opts = new List<string> { "Add Product", "Edit Product", "Delete Product", "View All Products", "Search / Scan (name/brand/barcode)", "Back" };
            while (true)
            {
                int sel = MenuSelect(opts, "PRODUCTS");
                if (sel == -1 || sel == opts.Count - 1) return;
                switch (sel)
                {
                    case 0: AddProduct(); break;
                    case 1: EditProduct(); break;
                    case 2: DeleteProduct(); break;
                    case 3: ViewAllProducts(); break;
                    case 4: SearchOrScanProduct(); break;
                }
            }
        }

        static void AddProduct()
        {
            Console.Clear();
            WriteBoxedTitle("PRODUCTS", "ADD PRODUCT");
            Console.WriteLine("Press 0 at any prompt to cancel and go back.\n");
            Console.Write("Name: ");
            var name = Console.ReadLine();
            if (name == "0") return;
            Console.Write("Brand: ");
            var brand = Console.ReadLine();

            var cat = CategorySelectArrow("Choose category", allowKeep: false, currentCategory: "");
            if (cat == null) return;

            int stock = ReadIntWithBack("Stock (enter number): ", allowBack: true);
            if (stock == int.MinValue) return;
            double price = ReadDoubleWithBack("Price: ", allowBack: true);
            if (double.IsNaN(price)) return;
            Console.Write("Barcode (leave blank to auto-generate): ");
            var barcode = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(barcode)) barcode = GenerateBarcode();
            if (products.Any(p => p.Barcode.Equals(barcode, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Barcode already exists. Press any key...");
                Console.ReadKey();
                return;
            }

            Product prod;
            if (cat.Equals("Sneakers", StringComparison.OrdinalIgnoreCase))
                prod = new SneakerProduct(nextProductId++, name.Trim(), brand?.Trim(), cat, stock, price, barcode.Trim());
            else if (cat.Equals("Watches", StringComparison.OrdinalIgnoreCase))
                prod = new WatchProduct(nextProductId++, name.Trim(), brand?.Trim(), cat, stock, price, barcode.Trim());
            else if (cat.Equals("Streetwear", StringComparison.OrdinalIgnoreCase))
                prod = new StreetwearProduct(nextProductId++, name.Trim(), brand?.Trim(), cat, stock, price, barcode.Trim());
            else
                prod = new Product(nextProductId++, name.Trim(), brand?.Trim(), cat, stock, price, barcode.Trim());

            products.Add(prod);
            SaveProducts();

            try
            {
                SaveInventoryReceipt_ProductAdded(prod, CurrentAdmin);
            }
            catch {}

            Console.WriteLine("\nProduct added. Press any key...");
            Console.ReadKey();
        }

        static void EditProduct()
        {
            while (true)
            {
                Console.Clear();
                WriteBoxedTitle("PRODUCTS", "EDIT PRODUCT");
                Console.WriteLine("0) Back\n");
                ViewProductsBrief();
                Console.Write("\nEnter Product ID to edit: ");
                var input = Console.ReadLine();
                if (input == "0") return;
                if (!int.TryParse(input, out var id))
                {
                    Console.WriteLine("Invalid. Press any key..."); Console.ReadKey(); continue;
                }
                var p = products.FirstOrDefault(x => x.Id == id);
                if (p == null) { Console.WriteLine("Not found. Press any key..."); Console.ReadKey(); continue; }

                var oldName = p.Name;
                var oldBrand = p.Brand;
                var oldCategory = p.Category;
                var oldPrice = p.Price;
                var oldStock = p.Stock;
                var oldBarcode = p.Barcode;

                Console.WriteLine($"Leave blank to keep current value.");
                Console.Write($"Name ({p.Name}): ");
                var s = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(s)) p.Name = s.Trim();
                Console.Write($"Brand ({p.Brand}): ");
                s = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(s)) p.Brand = s.Trim();

                Console.WriteLine($"Category ({p.Category}): ");
                var cat = CategorySelectArrow("Choose new category (or Back / Keep current)", allowKeep: true, currentCategory: p.Category);
                if (cat == null) {  }
                else if (cat == "") { }
                else
                {
                 
                    Product newProd;
                    if (cat.Equals("Sneakers", StringComparison.OrdinalIgnoreCase))
                        newProd = new SneakerProduct(p.Id, p.Name, p.Brand, cat, p.Stock, p.Price, p.Barcode);
                    else if (cat.Equals("Watches", StringComparison.OrdinalIgnoreCase))
                        newProd = new WatchProduct(p.Id, p.Name, p.Brand, cat, p.Stock, p.Price, p.Barcode);
                    else if (cat.Equals("Streetwear", StringComparison.OrdinalIgnoreCase))
                        newProd = new StreetwearProduct(p.Id, p.Name, p.Brand, cat, p.Stock, p.Price, p.Barcode);
                    else
                        newProd = new Product(p.Id, p.Name, p.Brand, cat, p.Stock, p.Price, p.Barcode);

                   
                    newProd.Name = p.Name;
                    newProd.Brand = p.Brand;
                    newProd.Price = p.Price;
                    newProd.Stock = p.Stock;
                    newProd.Barcode = p.Barcode;

                    var idx = products.IndexOf(p);
                    if (idx >= 0) products[idx] = newProd;
                    p = newProd; 
                }

                Console.Write($"Price ({p.Price}): ");
                var pr = Console.ReadLine(); if (double.TryParse(pr, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv)) p.Price = dv;
                Console.Write($"Stock ({p.Stock}): ");
                var st = Console.ReadLine(); if (int.TryParse(st, out var sv)) p.Stock = sv;
                Console.Write($"Barcode ({p.Barcode}): ");
                var bc = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(bc))
                {
                    if (products.Any(x => x.Barcode == bc && x.Id != p.Id))
                        Console.WriteLine("Barcode already used by another product. Keeping old barcode.");
                    else p.Barcode = bc.Trim();
                }
                SaveProducts();

                try
                {
                    SaveInventoryReceipt_ProductEdited(p, CurrentAdmin,
                        oldName, oldBrand, oldCategory, oldStock, oldPrice, oldBarcode);
                }
                catch { }

                Console.WriteLine("Product updated. Press any key...");
                Console.ReadKey();
                return;
            }
        }

        static void DeleteProduct()
        {
            while (true)
            {
                Console.Clear();
                WriteBoxedTitle("PRODUCTS", "DELETE PRODUCT");
                Console.WriteLine("0) Back\n");
                ViewProductsBrief();
                Console.Write("\nEnter Product ID to delete or 0 to go back: ");
                var input = Console.ReadLine();
                if (input == "0") return;
                if (!int.TryParse(input, out var id)) { Console.WriteLine("Invalid. Press any key..."); Console.ReadKey(); continue; }
                var p = products.FirstOrDefault(x => x.Id == id);
                if (p == null) { Console.WriteLine("Not found. Press any key..."); Console.ReadKey(); continue; }
                Console.Write($"Are you sure you want to delete '{p.Name}'? (y/n): ");
                var c = Console.ReadLine();
                if (c?.ToLower() == "y")
                {
                    Product copy;
                    if (p.Category.Equals("Sneakers", StringComparison.OrdinalIgnoreCase))
                        copy = new SneakerProduct(p.Id, p.Name, p.Brand, p.Category, p.Stock, p.Price, p.Barcode);
                    else if (p.Category.Equals("Watches", StringComparison.OrdinalIgnoreCase))
                        copy = new WatchProduct(p.Id, p.Name, p.Brand, p.Category, p.Stock, p.Price, p.Barcode);
                    else if (p.Category.Equals("Streetwear", StringComparison.OrdinalIgnoreCase))
                        copy = new StreetwearProduct(p.Id, p.Name, p.Brand, p.Category, p.Stock, p.Price, p.Barcode);
                    else
                        copy = new Product(p.Id, p.Name, p.Brand, p.Category, p.Stock, p.Price, p.Barcode);

                    products.Remove(p);
                    SaveProducts();

                    try
                    {
                        SaveInventoryReceipt_ProductDeleted(copy, CurrentAdmin);
                    }
                    catch { }

                    Console.WriteLine("Deleted. Press any key...");
                    Console.ReadKey();
                    return;
                }
                else { Console.WriteLine("Cancelled. Press any key..."); Console.ReadKey(); return; }
            }
        }

        static void ViewAllProducts()
        {
            Console.Clear();
            WriteBoxedTitle("PRODUCTS", "ALL PRODUCTS");
            if (!products.Any()) Console.WriteLine("No products available.");
            else
            {
                Console.WriteLine("Id | Name | Brand | Category | Stock | Price | Barcode");
                Console.WriteLine(new string('-', 100));
                foreach (var p in products)
                {
                    var low = p.Stock <= 5 ? " <- LOW" : "";
                    Console.WriteLine($"{p.Id} | {p.Name} | {p.Brand} | {p.Category} | {p.Stock} | ₱{p.Price.ToString("N2", CultureInfo.InvariantCulture)} | {p.Barcode}{low}");
                }
            }
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }

        static void SearchOrScanProduct()
        {
            var opts = new List<string> { "Search by name / brand", "Scan / enter barcode", "Back" };
            while (true)
            {
                int sel = MenuSelect(opts, "PRODUCTS - SEARCH / SCAN");
                if (sel == -1 || sel == opts.Count - 1) return;
                if (sel == 0)
                {
                    Console.Clear();
                    WriteBoxedTitle("PRODUCTS", "SEARCH (name / brand)");
                    Console.Write("Enter keyword (or 0 to back): ");
                    var kw = Console.ReadLine();
                    if (kw == "0") continue;
                    var res = products.Where(p => p.Name.IndexOf(kw ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                               || p.Brand.IndexOf(kw ?? "", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    Console.Clear();
                    WriteBoxedTitle("PRODUCTS", "SEARCH RESULTS");
                    if (!res.Any()) Console.WriteLine("No results.");
                    else foreach (var p in res) Console.WriteLine($"{p.Id} | {p.Name} | {p.Brand} | {p.Category} | {p.Stock} | ₱{p.Price}");
                    Console.WriteLine("\nPress any key...");
                    Console.ReadKey();
                }
                else if (sel == 1)
                {
                    Console.Clear();
                    WriteBoxedTitle("PRODUCTS", "SCAN / BARCODE");
                    Console.Write("Enter barcode (or 0 to back): ");
                    var bc = Console.ReadLine();
                    if (bc == "0") continue;
                    var p = products.FirstOrDefault(x => x.Barcode.Equals(bc, StringComparison.OrdinalIgnoreCase));
                    Console.Clear();
                    WriteBoxedTitle("PRODUCTS", "SCAN RESULT");
                    if (p == null) Console.WriteLine("Product not found for that barcode.");
                    else Console.WriteLine($"{p.Id} | {p.Name} | {p.Brand} | {p.Category} | {p.Stock} | ₱{p.Price}");
                    Console.WriteLine("\nPress any key...");
                    Console.ReadKey();
                }
            }
        }
        #endregion

        #region Customers
        static void CustomersMenu()
        {
            var opts = new List<string> { "Add Customer", "Edit Customer", "Delete Customer", "View Customers", "Back" };
            while (true)
            {
                int sel = MenuSelect(opts, "CUSTOMERS");
                if (sel == -1 || sel == opts.Count - 1) return;
                switch (sel)
                {
                    case 0: AddCustomer(); break;
                    case 1: EditCustomer(); break;
                    case 2: DeleteCustomer(); break;
                    case 3: ViewCustomers(); break;
                }
            }
        }

        static void AddCustomer()
        {
            Console.Clear();
            WriteBoxedTitle("CUSTOMERS", "ADD CUSTOMER");
            Console.WriteLine("0) Back");
            Console.Write("Name: ");
            var name = Console.ReadLine();
            if (name == "0") return;
            Console.Write("Contact: ");
            var contact = Console.ReadLine();
            var c = new Customer(nextCustomerId++, name.Trim(), contact?.Trim());
            customers.Add(c);
            SaveCustomers();
            Console.WriteLine("Customer added. Press any key...");
            Console.ReadKey();
        }

        static void EditCustomer()
        {
            Console.Clear();
            WriteBoxedTitle("CUSTOMERS", "EDIT CUSTOMER");
            Console.WriteLine("0) Back\n");
            ViewCustomersBrief();
            Console.Write("\nEnter customer ID: ");
            var s = Console.ReadLine(); if (s == "0") return;
            if (!int.TryParse(s, out var id)) { Console.WriteLine("Invalid. Press any key..."); Console.ReadKey(); return; }
            var c = customers.FirstOrDefault(x => x.Id == id);
            if (c == null) { Console.WriteLine("Not found. Press any key..."); Console.ReadKey(); return; }
            Console.Write($"Name ({c.Name}): "); var n = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(n)) c.Name = n.Trim();
            Console.Write($"Contact ({c.Contact}): "); var co = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(co)) c.Contact = co.Trim();
            SaveCustomers(); Console.WriteLine("Updated. Press any key..."); Console.ReadKey();
        }

        static void DeleteCustomer()
        {
            Console.Clear();
            WriteBoxedTitle("CUSTOMERS", "DELETE CUSTOMER");
            Console.WriteLine("0) Back\n");
            ViewCustomersBrief();
            Console.Write("\nEnter customer ID: ");
            var s = Console.ReadLine(); if (s == "0") return;
            if (!int.TryParse(s, out var id)) { Console.WriteLine("Invalid. Press any key..."); Console.ReadKey(); return; }
            var c = customers.FirstOrDefault(x => x.Id == id);
            if (c == null) { Console.WriteLine("Not found. Press any key..."); Console.ReadKey(); return; }
            customers.Remove(c); SaveCustomers(); Console.WriteLine("Deleted. Press any key..."); Console.ReadKey();
        }

        static void ViewCustomers()
        {
            Console.Clear();
            WriteBoxedTitle("CUSTOMERS", "ALL CUSTOMERS");
            if (!customers.Any()) Console.WriteLine("No customers.");
            else
            {
                Console.WriteLine("Id | Name | Contact");
                Console.WriteLine(new string('-', 60));
                foreach (var c in customers) Console.WriteLine($"{c.Id} | {c.Name} | {c.Contact}");
            }
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }

        static void ViewCustomersBrief()
        {
            if (!customers.Any()) Console.WriteLine("No customers.");
            else
            {
                Console.WriteLine("Id | Name | Contact");
                Console.WriteLine(new string('-', 40));
                foreach (var c in customers) Console.WriteLine($"{c.Id} | {c.Name} | {c.Contact}");
            }
        }
        #endregion

        #region Suppliers
        static void SuppliersMenu()
        {
            var opts = new List<string> { "Add Supplier", "Edit Supplier", "Delete Supplier", "View Suppliers", "Back" };
            while (true)
            {
                int sel = MenuSelect(opts, "SUPPLIERS");
                if (sel == -1 || sel == opts.Count - 1) return;
                switch (sel)
                {
                    case 0: AddSupplier(); break;
                    case 1: EditSupplier(); break;
                    case 2: DeleteSupplier(); break;
                    case 3: ViewSuppliers(); break;
                }
            }
        }

        static void AddSupplier()
        {
            Console.Clear();
            WriteBoxedTitle("SUPPLIERS", "ADD SUPPLIER");
            Console.WriteLine("0) Back");
            Console.Write("Name: ");
            var name = Console.ReadLine(); if (name == "0") return;
            Console.Write("Contact: ");
            var contact = Console.ReadLine();
            var s = new Supplier(nextSupplierId++, name.Trim(), contact?.Trim());
            suppliers.Add(s); SaveSuppliers(); Console.WriteLine("Supplier added. Press any key..."); Console.ReadKey();
        }

        static void EditSupplier()
        {
            Console.Clear();
            WriteBoxedTitle("SUPPLIERS", "EDIT SUPPLIER");
            Console.WriteLine("0) Back\n");
            ViewSuppliersBrief();
            Console.Write("\nEnter supplier ID: ");
            var ss = Console.ReadLine(); if (ss == "0") return;
            if (!int.TryParse(ss, out var id)) { Console.WriteLine("Invalid. Press any key..."); Console.ReadKey(); return; }
            var s = suppliers.FirstOrDefault(x => x.Id == id);
            if (s == null) { Console.WriteLine("Not found. Press any key..."); Console.ReadKey(); return; }
            Console.Write($"Name ({s.Name}): "); var n = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(n)) s.Name = n.Trim();
            Console.Write($"Contact ({s.Contact}): "); var c = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(c)) s.Contact = c.Trim();
            SaveSuppliers(); Console.WriteLine("Updated. Press any key..."); Console.ReadKey();
        }

        static void DeleteSupplier()
        {
            Console.Clear();
            WriteBoxedTitle("SUPPLIERS", "DELETE SUPPLIER");
            Console.WriteLine("0) Back\n");
            ViewSuppliersBrief();
            Console.Write("\nEnter supplier ID: ");
            var ss = Console.ReadLine(); if (ss == "0") return;
            if (!int.TryParse(ss, out var id)) { Console.WriteLine("Invalid. Press any key..."); Console.ReadKey(); return; }
            var s = suppliers.FirstOrDefault(x => x.Id == id);
            if (s == null) { Console.WriteLine("Not found. Press any key..."); Console.ReadKey(); return; }
            suppliers.Remove(s); SaveSuppliers(); Console.WriteLine("Deleted. Press any key..."); Console.ReadKey();
        }

        static void ViewSuppliers()
        {
            Console.Clear();
            WriteBoxedTitle("SUPPLIERS", "ALL SUPPLIERS");
            if (!suppliers.Any()) Console.WriteLine("No suppliers.");
            else
            {
                Console.WriteLine("Id | Name | Contact");
                Console.WriteLine(new string('-', 60));
                foreach (var s in suppliers) Console.WriteLine($"{s.Id} | {s.Name} | {s.Contact}");
            }
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }

        static void ViewSuppliersBrief()
        {
            if (!suppliers.Any()) Console.WriteLine("No suppliers.");
            else
            {
                Console.WriteLine("Id | Name | Contact");
                Console.WriteLine(new string('-', 40));
                foreach (var s in suppliers) Console.WriteLine($"{s.Id} | {s.Name} | {s.Contact}");
            }
        }
        #endregion

        #region Sales & Purchases (invoice file creation)
        static void SalesMenu(AdminUser admin)
        {
            var opts = new List<string> { "New Sale / Checkout", "View Sales History", "Back" };
            while (true)
            {
                int sel = MenuSelect(opts, "SALES");
                if (sel == -1 || sel == opts.Count - 1) return;
                switch (sel)
                {
                    case 0: NewSale(admin); break;
                    case 1: ViewSalesHistory(); break;
                }
            }
        }

        static void NewSale(AdminUser admin)
        {
            Console.Clear();
            WriteBoxedTitle("SALES", "NEW SALE / CHECKOUT");
            Console.WriteLine("Enter customer name or type 'list' to choose existing customer, or leave blank for Walk-in:");
            Console.Write("Customer: ");
            var inpt = Console.ReadLine();
            if (inpt?.ToLower() == "list")
            {
                ViewCustomersBrief();
                Console.Write("\nEnter customer ID or 0 to cancel: ");
                var s = Console.ReadLine();
                if (s == "0") return;
                if (int.TryParse(s, out var cid))
                {
                    var c = customers.FirstOrDefault(x => x.Id == cid);
                    if (c != null) inpt = c.Name;
                }
            }
            string customerName = string.IsNullOrWhiteSpace(inpt) ? "Walk-in" : inpt.Trim();

            var invoice = new Invoice(nextSaleId++, "Sale", customerName);

            while (true)
            {
                Console.Clear();
                WriteBoxedTitle("SALES", $"Building sale for: {customerName}");
                Console.WriteLine("Enter product barcode or product ID (or type 'list' to show products, 'done' to finish, 'cancel' to abort):");
                Console.Write("> ");
                var cmd = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmd)) continue;
                if (cmd.ToLower() == "done")
                {
                    if (!invoice.Items.Any()) { Console.WriteLine("No items added. Sale canceled. Press any key..."); Console.ReadKey(); return; }
                    // complete sale: deduct stock, save
                    foreach (var it in invoice.Items)
                    {
                        var prod = products.FirstOrDefault(p => p.Id == it.ProductId);
                        if (prod != null) prod.Stock -= it.Quantity;
                    }
                    sales.Add(invoice);
                    SaveSales();
                    SaveProducts();

                    // Create invoice text file quietly
                    try
                    {
                        SaveInvoiceToFile(invoice, admin.Username);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: failed to write invoice file: {ex.Message}");
                    }

                    Console.WriteLine($"\nSale saved. Invoice #{invoice.Id} Total: ₱{invoice.GrandTotal.ToString("N2", CultureInfo.InvariantCulture)}");
                    Console.WriteLine("Printable invoice quietly saved. Press any key...");
                    Console.ReadKey();
                    ShowLowStockAlert();
                    return;
                }
                if (cmd.ToLower() == "cancel") { Console.WriteLine("Sale canceled. Press any key..."); Console.ReadKey(); return; }
                if (cmd.ToLower() == "list") { ViewProductsBrief(); Console.WriteLine("\nPress any key..."); Console.ReadKey(); continue; }

                Product prodFound = null;
                if (int.TryParse(cmd, out var pid))
                    prodFound = products.FirstOrDefault(p => p.Id == pid);
                else
                    prodFound = products.FirstOrDefault(p => p.Barcode.Equals(cmd, StringComparison.OrdinalIgnoreCase));

                if (prodFound == null) { Console.WriteLine("Product not found. Press any key..."); Console.ReadKey(); continue; }
                Console.WriteLine($"{prodFound.Id} | {prodFound.Name} | Price: ₱{prodFound.Price} | Stock: {prodFound.Stock}");
                Console.Write("Quantity to sell (or 0 to cancel): ");
                if (!int.TryParse(Console.ReadLine(), out var qty) || qty <= 0) { Console.WriteLine("Cancelled or invalid qty. Press any key..."); Console.ReadKey(); continue; }
                if (qty > prodFound.Stock) { Console.WriteLine("Not enough stock. Press any key..."); Console.ReadKey(); continue; }
                invoice.Items.Add(new InvoiceItem(prodFound.Id, prodFound.Name, qty, prodFound.Price));
                Console.WriteLine("Item added. Press any key to continue adding or type 'done' when finished...");
                Console.ReadKey();
            }
        }

        static void ViewSalesHistory()
        {
            Console.Clear();
            WriteBoxedTitle("SALES", "SALES HISTORY");
            if (!sales.Any()) { Console.WriteLine("No sales recorded."); Console.WriteLine("\nPress any key to go back..."); Console.ReadKey(); return; }
            foreach (var s in sales.OrderByDescending(x => x.Date))
            {
                Console.WriteLine($"Invoice #{s.Id} | {s.Date} | {s.Party} | Total: ₱{s.GrandTotal.ToString("N2", CultureInfo.InvariantCulture)}");
                foreach (var it in s.Items) Console.WriteLine($"   - {it.ProductName} x{it.Quantity} @ ₱{it.UnitPrice.ToString("N2", CultureInfo.InvariantCulture)} = ₱{it.Total.ToString("N2", CultureInfo.InvariantCulture)}");
                Console.WriteLine(new string('-', 60));
            }
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }

        static void PurchasesMenu(AdminUser admin)
        {
            var opts = new List<string> { "New Purchase (stock in)", "View Purchase History", "Back" };
            while (true)
            {
                int sel = MenuSelect(opts, "PURCHASES");
                if (sel == -1 || sel == opts.Count - 1) return;
                switch (sel)
                {
                    case 0: NewPurchase(admin); break;
                    case 1: ViewPurchaseHistory(); break;
                }
            }
        }

        static void NewPurchase(AdminUser admin)
        {
            Console.Clear();
            WriteBoxedTitle("PURCHASES", "NEW PURCHASE");
            Console.WriteLine("Enter supplier name or type 'list' to choose existing supplier, or leave blank:");
            Console.Write("Supplier: ");
            var inpt = Console.ReadLine();
            if (inpt?.ToLower() == "list")
            {
                ViewSuppliersBrief();
                Console.Write("\nEnter supplier ID or 0 to cancel: ");
                var s = Console.ReadLine();
                if (s == "0") return;
                if (int.TryParse(s, out var sid))
                {
                    var sup = suppliers.FirstOrDefault(x => x.Id == sid);
                    if (sup != null) inpt = sup.Name;
                }
            }
            string supplierName = string.IsNullOrWhiteSpace(inpt) ? "Unknown Supplier" : inpt.Trim();
            var invoice = new Invoice(nextPurchaseId++, "Purchase", supplierName);

            while (true)
            {
                Console.Clear();
                WriteBoxedTitle("PURCHASES", $"Building purchase from: {supplierName}");
                Console.WriteLine("Enter product barcode or product ID (or 'list', 'done', 'cancel'):");
                Console.Write("> ");
                var cmd = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmd)) continue;
                if (cmd.ToLower() == "done")
                {
                    if (!invoice.Items.Any()) { Console.WriteLine("No items. Purchase canceled. Press any key..."); Console.ReadKey(); return; }
                    // complete: add stock
                    foreach (var it in invoice.Items)
                    {
                        var prod = products.FirstOrDefault(p => p.Id == it.ProductId);
                        if (prod != null) prod.Stock += it.Quantity;
                    }
                    purchases.Add(invoice);
                    SavePurchases();
                    SaveProducts();

                    // Create inventory receipt for this purchase with list of items
                    try
                    {
                        SaveInventoryReceipt_Purchase(invoice, admin.Username);
                    }
                    catch { }

                    Console.WriteLine($"\nPurchase saved. Invoice #{invoice.Id} Total: ₱{invoice.GrandTotal.ToString("N2", CultureInfo.InvariantCulture)}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }
                if (cmd.ToLower() == "cancel") { Console.WriteLine("Purchase canceled. Press any key..."); Console.ReadKey(); return; }
                if (cmd.ToLower() == "list") { ViewProductsBrief(); Console.WriteLine("\nPress any key..."); Console.ReadKey(); continue; }

                Product prodFound = null;
                if (int.TryParse(cmd, out var pid))
                    prodFound = products.FirstOrDefault(p => p.Id == pid);
                else
                    prodFound = products.FirstOrDefault(p => p.Barcode.Equals(cmd, StringComparison.OrdinalIgnoreCase));

                if (prodFound == null) { Console.WriteLine("Product not found. Press any key..."); Console.ReadKey(); continue; }
                Console.WriteLine($"{prodFound.Id} | {prodFound.Name} | Price: ₱{prodFound.Price} | Stock: {prodFound.Stock}");
                Console.Write("Quantity to add (or 0 to cancel): ");
                if (!int.TryParse(Console.ReadLine(), out var qty) || qty <= 0) { Console.WriteLine("Cancelled or invalid qty. Press any key..."); Console.ReadKey(); continue; }
                invoice.Items.Add(new InvoiceItem(prodFound.Id, prodFound.Name, qty, prodFound.Price));
                Console.WriteLine("Item added. Press any key to continue adding or type 'done' when finished...");
                Console.ReadKey();
            }
        }

        static void ViewPurchaseHistory()
        {
            Console.Clear();
            WriteBoxedTitle("PURCHASES", "PURCHASE HISTORY");
            if (!purchases.Any()) { Console.WriteLine("No purchases recorded."); Console.WriteLine("\nPress any key to go back..."); Console.ReadKey(); return; }
            foreach (var s in purchases.OrderByDescending(x => x.Date))
            {
                Console.WriteLine($"Invoice #{s.Id} | {s.Date} | {s.Party} | Total: ₱{s.GrandTotal.ToString("N2", CultureInfo.InvariantCulture)}");
                foreach (var it in s.Items) Console.WriteLine($"   - {it.ProductName} x{it.Quantity} @ ₱{it.UnitPrice.ToString("N2", CultureInfo.InvariantCulture)} = ₱{it.Total.ToString("N2", CultureInfo.InvariantCulture)}");
                Console.WriteLine(new string('-', 60));
            }
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }
        #endregion

        #region Reports & LowStock
        static void ReportsMenu()
        {
            var opts = new List<string> { "Summary (totals)", "Top selling items", "Inventory value", "Back" };
            while (true)
            {
                int sel = MenuSelect(opts, "REPORTS");
                if (sel == -1 || sel == opts.Count - 1) return;
                switch (sel)
                {
                    case 0: ReportsSummary(); break;
                    case 1: ReportsTopSelling(); break;
                    case 2: ReportsInventoryValue(); break;
                }
            }
        }

        static void ReportsSummary()
        {
            Console.Clear();
            WriteBoxedTitle("REPORTS", "SUMMARY");
            var totalSales = sales.Sum(s => s.GrandTotal);
            var totalPurchases = purchases.Sum(p => p.GrandTotal);
            var totalProducts = products.Count;
            var lowStockCount = products.Count(p => p.Stock <= 5);
            var totalCustomers = customers.Count;
            var totalSuppliers = suppliers.Count;

            Console.WriteLine($"Total Products: {totalProducts}");
            Console.WriteLine($"Total Customers: {totalCustomers}");
            Console.WriteLine($"Total Suppliers: {totalSuppliers}");
            Console.WriteLine($"Total Sales (grand): ₱{totalSales.ToString("N2", CultureInfo.InvariantCulture)}");
            Console.WriteLine($"Total Purchases (grand): ₱{totalPurchases.ToString("N2", CultureInfo.InvariantCulture)}");
            Console.WriteLine($"Low-stock Products (<=5): {lowStockCount}");
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }

        static void ReportsTopSelling()
        {
            Console.Clear();
            WriteBoxedTitle("REPORTS", "TOP SELLING ITEMS");
            var itemCounts = new Dictionary<int, int>();
            foreach (var s in sales)
                foreach (var it in s.Items)
                    itemCounts[it.ProductId] = itemCounts.GetValueOrDefault(it.ProductId, 0) + it.Quantity;

            var top = itemCounts.OrderByDescending(kv => kv.Value).Take(10);
            if (!top.Any()) Console.WriteLine("No sales data yet.");
            else
            {
                Console.WriteLine("Qty | Product");
                foreach (var kv in top)
                {
                    var prod = products.FirstOrDefault(p => p.Id == kv.Key);
                    Console.WriteLine($"{kv.Value} | {prod?.Name ?? "Unknown Product"}");
                }
            }
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }

        static void ReportsInventoryValue()
        {
            Console.Clear();
            WriteBoxedTitle("REPORTS", "INVENTORY VALUE");
            double totalValue = products.Sum(p => p.Price * p.Stock);
            Console.WriteLine($"Total inventory value: ₱{totalValue.ToString("N2", CultureInfo.InvariantCulture)}");
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }

        static void ShowLowStockAlert()
        {
            var low = products.Where(p => p.Stock <= 5).ToList();
            if (low.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n⚠ LOW STOCK ALERT ⚠");
                foreach (var p in low) Console.WriteLine($"• {p.Name} ({p.Stock} pcs left)");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        static void ViewLowStockMenu()
        {
            Console.Clear();
            WriteBoxedTitle("INVENTORY", "LOW STOCK (<=5)");
            var low = products.Where(p => p.Stock <= 5).ToList();
            if (!low.Any()) Console.WriteLine("No low stock products.");
            else foreach (var p in low) Console.WriteLine($"{p.Id} | {p.Name} | {p.Brand} | {p.Stock} pcs");
            Console.WriteLine("\nPress any key to go back...");
            Console.ReadKey();
        }
        #endregion

        #region Invoice file creation
        static void SaveInvoiceToFile(Invoice invoice, string cashier)
        {
            try
            {
                Directory.CreateDirectory(InvoicesFolder);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"invoice_{timestamp}.txt";
                var path = Path.Combine(InvoicesFolder, filename);

                var sb = new StringBuilder();
                sb.AppendLine("========================================");
                sb.AppendLine("           JB SNEAKERS & APPAREL        ");
                sb.AppendLine("           OFFICIAL SALES INVOICE       ");
                sb.AppendLine("========================================");
                sb.AppendLine($"Invoice #: {invoice.Id}");
                sb.AppendLine($"Date: {invoice.Date.ToString("yyyy-MM-dd HH:mm:ss")}");
                sb.AppendLine($"Cashier: {cashier}");
                sb.AppendLine($"Customer: {invoice.Party}");
                sb.AppendLine("----------------------------------------");
                sb.AppendLine("Item                          Qty   Subtotal");
                sb.AppendLine("----------------------------------------");
                foreach (var it in invoice.Items)
                {
                    var name = it.ProductName.Length > 25 ? it.ProductName.Substring(0, 25) : it.ProductName;
                    sb.AppendLine($"{name.PadRight(30)} {it.Quantity.ToString().PadLeft(3)}   ₱{it.Total.ToString("N2", CultureInfo.InvariantCulture).PadLeft(8)}");
                }
                sb.AppendLine("----------------------------------------");
                sb.AppendLine($"GRAND TOTAL: ₱{invoice.GrandTotal.ToString("N2", CultureInfo.InvariantCulture)}");
                sb.AppendLine("----------------------------------------");
                sb.AppendLine("Thank you for shopping with us!");
                sb.AppendLine("JB SNEAKERS & APPAREL");
                sb.AppendLine("========================================");

                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing invoice file: {ex.Message}", ex);
            }
        }
        #endregion

        #region Inventory receipt creation (single + batch)
        static void TryOpenInventoryReceiptsFolder()
        {
            try
            {
                var full = Path.GetFullPath(InventoryReceiptsFolder);
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{full}\"") { UseShellExecute = true });
            }
            catch
            {
                // ignore open failures
            }
        }

        static void SaveInventoryReceipt_ProductAdded(Product p, string admin)
        {
            Directory.CreateDirectory(InventoryReceiptsFolder);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"receipt_{timestamp}.txt";
            var path = Path.Combine(InventoryReceiptsFolder, filename);

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("   JB SNEAKERS & APPAREL - INVENTORY    ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"Action: PRODUCT ADDED");
            sb.AppendLine($"Admin: {admin}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Product ID: {p.Id}");
            sb.AppendLine($"Product Name: {p.Name}");
            sb.AppendLine($"Brand: {p.Brand}");
            sb.AppendLine($"Category: {p.Category}");
            sb.AppendLine($"Stock: {p.Stock}");
            sb.AppendLine($"Price: ₱{p.Price.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Barcode: {p.Barcode}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Status: SUCCESSFULLY SAVED");
            sb.AppendLine("========================================");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            TryOpenInventoryReceiptsFolder();
        }

        static void SaveInventoryReceipt_ProductEdited(Product p, string admin,
            string oldName, string oldBrand, string oldCategory, int oldStock, double oldPrice, string oldBarcode)
        {
            Directory.CreateDirectory(InventoryReceiptsFolder);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"receipt_{timestamp}.txt";
            var path = Path.Combine(InventoryReceiptsFolder, filename);

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("   JB SNEAKERS & APPAREL - INVENTORY    ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"Action: PRODUCT EDITED");
            sb.AppendLine($"Admin: {admin}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Product ID: {p.Id}");
            sb.AppendLine($"Old Name: {oldName}");
            sb.AppendLine($"New Name: {p.Name}");
            sb.AppendLine($"Old Brand: {oldBrand}");
            sb.AppendLine($"New Brand: {p.Brand}");
            sb.AppendLine($"Old Category: {oldCategory}");
            sb.AppendLine($"New Category: {p.Category}");
            sb.AppendLine($"Old Stock: {oldStock}");
            sb.AppendLine($"New Stock: {p.Stock}");
            sb.AppendLine($"Old Price: ₱{oldPrice.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"New Price: ₱{p.Price.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Old Barcode: {oldBarcode}");
            sb.AppendLine($"New Barcode: {p.Barcode}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Status: SUCCESSFULLY SAVED");
            sb.AppendLine("========================================");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            TryOpenInventoryReceiptsFolder();
        }

        static void SaveInventoryReceipt_ProductDeleted(Product p, string admin)
        {
            Directory.CreateDirectory(InventoryReceiptsFolder);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"receipt_{timestamp}.txt";
            var path = Path.Combine(InventoryReceiptsFolder, filename);

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("   JB SNEAKERS & APPAREL - INVENTORY    ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"Action: PRODUCT DELETED");
            sb.AppendLine($"Admin: {admin}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Product ID: {p.Id}");
            sb.AppendLine($"Product Name: {p.Name}");
            sb.AppendLine($"Brand: {p.Brand}");
            sb.AppendLine($"Category: {p.Category}");
            sb.AppendLine($"Stock: {p.Stock}");
            sb.AppendLine($"Price: ₱{p.Price.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Barcode: {p.Barcode}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Status: SUCCESSFULLY DELETED");
            sb.AppendLine("========================================");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            TryOpenInventoryReceiptsFolder();
        }

        static void SaveInventoryReceipt_StockAdjusted(Product p, string admin, int oldStock, int newStock, string remarks = "")
        {
            Directory.CreateDirectory(InventoryReceiptsFolder);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"receipt_{timestamp}.txt";
            var path = Path.Combine(InventoryReceiptsFolder, filename);

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("   JB SNEAKERS & APPAREL - INVENTORY    ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"Action: STOCK UPDATED");
            sb.AppendLine($"Admin: {admin}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Product ID: {p.Id}");
            sb.AppendLine($"Product: {p.Name}");
            sb.AppendLine($"Category: {p.Category}");
            sb.AppendLine($"Old Stock: {oldStock}");
            sb.AppendLine($"New Stock: {newStock}");
            sb.AppendLine($"Price: ₱{p.Price.ToString("N2", CultureInfo.InvariantCulture)}");
            if (!string.IsNullOrWhiteSpace(remarks)) sb.AppendLine($"Remarks: {remarks}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Status: SUCCESSFULLY SAVED");
            sb.AppendLine("========================================");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            TryOpenInventoryReceiptsFolder();
        }

        static void SaveInventoryReceipt_Purchase(Invoice purchaseInvoice, string admin)
        {
            Directory.CreateDirectory(InventoryReceiptsFolder);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"receipt_{timestamp}.txt";
            var path = Path.Combine(InventoryReceiptsFolder, filename);

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("   JB SNEAKERS & APPAREL - INVENTORY    ");
            sb.AppendLine("              PURCHASE RECEIPT          ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"Action: PURCHASE (STOCK IN)");
            sb.AppendLine($"Admin: {admin}");
            sb.AppendLine($"Supplier: {purchaseInvoice.Party}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Items:");
            foreach (var it in purchaseInvoice.Items)
            {
                var prod = products.FirstOrDefault(p => p.Id == it.ProductId);
                var prevStock = prod != null ? prod.Stock - it.Quantity : 0; // approximate previous
                sb.AppendLine($"{it.ProductName} | Qty: {it.Quantity} | UnitPrice: ₱{it.UnitPrice.ToString("N2", CultureInfo.InvariantCulture)} | Subtotal: ₱{it.Total.ToString("N2", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"  Previous stock (approx): {Math.Max(0, prevStock)} | New stock (approx): {(prod != null ? prod.Stock.ToString() : "N/A")}");
            }
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"GRAND TOTAL: ₱{purchaseInvoice.GrandTotal.ToString("N2", CultureInfo.InvariantCulture)}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Status: PURCHASE RECORDED");
            sb.AppendLine("========================================");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            TryOpenInventoryReceiptsFolder();
        }
        #endregion

        #region Category selection (arrow-key controlled + Add New Category)
        // returns:
        // - null => Back/Cancel
        // - "" (empty string) => Keep current (only when allowKeep=true)
        // - category name => chosen/new category
        static string CategorySelectArrow(string title, bool allowKeep = false, string currentCategory = "")
        {
            // load categories
            var cats = File.Exists(CategoriesFile)
                ? File.ReadAllLines(CategoriesFile).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim()).ToList()
                : new List<string> { "Sneakers", "Watches", "Streetwear" };

            while (true)
            {
                // build menu options
                var opts = new List<string>();
                if (allowKeep)
                {
                    opts.Add("Keep current (" + currentCategory + ")");
                }
                opts.AddRange(cats);
                opts.Add("Add New Category");
                opts.Add("Back");

                int sel = MenuSelect(opts, $"CATEGORY - {title}");
                if (sel == -1) return null; // cancelled via ESC

                // Map selection
                int offset = 0;
                if (allowKeep) // first item is Keep current
                {
                    if (sel == 0) return ""; // keep
                    offset = 1;
                }

                if (sel >= offset && sel < offset + cats.Count)
                {
                    return cats[sel - offset];
                }

                // Add New Category
                if (sel == offset + cats.Count)
                {
                    Console.Clear();
                    WriteBoxedTitle("CATEGORIES", "ADD NEW CATEGORY");
                    Console.Write("Enter new category name (or 0 to cancel): ");
                    var newCat = Console.ReadLine();
                    if (newCat == "0") continue;
                    if (string.IsNullOrWhiteSpace(newCat))
                    {
                        Console.WriteLine("Invalid name. Press any key...");
                        Console.ReadKey();
                        continue;
                    }
                    newCat = newCat.Trim();
                    if (cats.Any(c => c.Equals(newCat, StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine("Category already exists. Press any key...");
                        Console.ReadKey();
                        continue;
                    }
                    cats.Add(newCat);
                    // save categories
                    SaveCategories(cats);
                    Console.WriteLine($"Category '{newCat}' added. Press any key...");
                    Console.ReadKey();
                    return newCat;
                }

                // Back selected
                if (sel == offset + cats.Count + 1)
                {
                    return null;
                }
            }
        }
        #endregion

        #region Helpers & UI utilities
        static int ReadIntWithBack(string prompt, bool allowBack = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (allowBack && s == "0") return int.MinValue;
                if (int.TryParse(s, out var i)) return i;
                Console.WriteLine("Invalid integer. Try again.");
            }
        }

        static double ReadDoubleWithBack(string prompt, bool allowBack = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (allowBack && s == "0") return double.NaN;
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
                Console.WriteLine("Invalid number. Try again.");
            }
        }

        static string GenerateBarcode()
        {
            var rnd = new Random();
            return $"BAR{DateTime.Now:yyyyMMddHHmmss}{rnd.Next(100, 999)}";
        }

        static void ViewProductsBrief()
        {
            if (!products.Any()) Console.WriteLine("No products.");
            else
            {
                Console.WriteLine("Id | Name | Category | Stock");
                Console.WriteLine(new string('-', 50));
                foreach (var p in products) Console.WriteLine($"{p.Id} | {p.Name} | {p.Category} | {p.Stock}");
            }
        }

        static int SimpleMenuSelect(List<string> options, string title)
        {
            int index = 0;
            ConsoleKeyInfo keyInfo;
            do
            {
                Console.Clear();
                WriteBoxedTitle("JB SNEAKERS & APPAREL", title);
                Console.WriteLine();
                for (int i = 0; i < options.Count; i++)
                {
                    if (i == index)
                    {
                        Console.BackgroundColor = SelectedBg;
                        Console.ForegroundColor = SelectedFg;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else Console.WriteLine($"  {options[i]}");
                }
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.UpArrow) index = (index == 0) ? options.Count - 1 : index - 1;
                else if (keyInfo.Key == ConsoleKey.DownArrow) index = (index == options.Count - 1) ? 0 : index + 1;
                else if (keyInfo.Key == ConsoleKey.Enter) return index;
                else if (keyInfo.Key == ConsoleKey.Escape) return -1;
            } while (true);
        }

        static string ChooseCategory(bool allowKeep = false)
        {
            // wrapper kept for compatibility - use CategorySelectArrow
            var current = allowKeep ? "" : "";
            return CategorySelectArrow("Choose category", allowKeep, current);
        }
        #endregion

        #region Boxed title renderer
        static void WriteBoxedTitle(string leftTitle, string subtitle)
        {
            int minWidth = Math.Max(leftTitle.Length, subtitle.Length) + 8;
            int width = Math.Min(Math.Max(50, minWidth), Math.Max(50, Console.WindowWidth - 4));
            string border = new string('═', width);
            Console.ForegroundColor = TitleBorderColor;
            Console.WriteLine("╔" + border + "╗");
            var leftCentered = leftTitle.PadLeft((width + leftTitle.Length) / 2).PadRight(width);
            Console.WriteLine("║" + leftCentered + "║");
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                var subCentered = subtitle.PadLeft((width + subtitle.Length) / 2).PadRight(width);
                Console.WriteLine("║" + subCentered + "║");
            }
            Console.WriteLine("╚" + border + "╝");
            Console.ResetColor();
        }
        #endregion
    }

    static class Extensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue @default = default)
        {
            if (dict.TryGetValue(key, out var val)) return val;
            return @default;
        }
    }
}

