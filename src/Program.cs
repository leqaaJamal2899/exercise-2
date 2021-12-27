using System.ComponentModel.Design;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder();
builder.Services.AddRazorPages();

var app = builder.Build();

Order order = new Order();

app.MapRazorPages();

app.MapGet("/", async () =>{
    var content = await GeneratePizzaPage();
    Menu menu = ReadMenuFromJSON();
    content = ChangeContent(content,menu);
    return Results.Text(content, "text/html");
});

app.MapPost("/TakePersonalInfo", ([FromBody] Person p) =>
{
    var response = new {
        GreetingText = "Helllo",
        Name = p.Name,
        Address = p.Address
    };
    order = new Order();
    order.UserName = p.Name;
    order.UserAddress = p.Address;
    order.ChosenPizzas = new List<Pizza>();
    return Results.Json(response);
});

app.MapPost("/TakePizza", ([FromBody] Pizza pizza) =>
{   
    var response = new {
        Type = pizza.PizzaType,
        Size = pizza.PizzaSize,
        Toppings = pizza.Toppings
    };
    Pizza pizza1 = new Pizza();
    pizza1.PizzaType = pizza.PizzaType;
    pizza1.PizzaSize = pizza.PizzaSize;
    pizza1.Toppings = pizza.Toppings;
    order.ChosenPizzas.Add(pizza);
    return Results.Json(response);
});

app.MapGet("/order", GetAcceptancePage);

IResult GetAcceptancePage()
{
    WriteOrderIntoJSON(order);
    var message = @"
        <html>
        <body>
            <h1>your order is received</h1>
        </body>
        </html>
    ";
    return Results.Text(message, "text/html");
}

app.Run();

static string ChangeContent(string content, Menu menu){
    string[] PizzaTypes = menu.PizzaTypes;
    string[] PizzaSizes = menu.PizzaSizes;
    string[] PizzaToppings = menu.PizzaToppings;
    string types = "";
    string sizes ="";
    string toppings = "";
    foreach(string pizzatype in PizzaTypes){
        types+= "<option value=" + "\""+pizzatype +"\""+ ">"+ pizzatype +"</option> "  ;
    }
    content = content.Replace("$PizzaType$",types);
    foreach(string pizzasize in PizzaSizes){
        sizes+= "<option value=" + "\""+pizzasize +"\""+ ">"+ pizzasize +"</option> "  ;
    }
    content = content.Replace("$PizzaSize$",sizes);
    foreach(string pizzatopping in PizzaToppings){
        toppings+= "<option value=" + "\""+pizzatopping +"\""+ ">"+ pizzatopping +"</option> "  ;
    }
    content = content.Replace("$PizzaToppings$",toppings);
    return content;
}

static void WriteOrderIntoJSON(Order order){
    string fileName = "madeOrders.json";
    var options = new JsonSerializerOptions { WriteIndented = true };
    List<Order> orders = readAllPreviousOrders();
    orders.Add(order);
    string jsonString = JsonSerializer.Serialize(orders, options);
    File.WriteAllText(fileName, jsonString);
}

static List<Order> readAllPreviousOrders(){
    string fileName = "madeOrders.json";
    string jsonString = File.ReadAllText(fileName);
    List<Order> orders;
    if(!jsonString.Equals(""))
        orders = JsonSerializer.Deserialize<List<Order>>(jsonString);
    else
        orders = new List<Order>();
    return orders;
}

static Menu ReadMenuFromJSON(){
    string fileName = "availableInMenu.json";
    string jsonString = File.ReadAllText(fileName);
    Menu menu = JsonSerializer.Deserialize<Menu>(jsonString);
    return menu;
}

async Task<string> GeneratePizzaPage()
{
    var path = Path.Combine(app.Environment.ContentRootPath, "ChoosingPizza.txt");
    var content = await File.ReadAllTextAsync(path);
    return content;
}

public class Menu
{
    public string[] PizzaTypes { get; set; }
    public string[] PizzaSizes { get; set; }
    public string[] PizzaToppings { get; set; }
}

public class Pizza
{
    public string PizzaType { get; set; }
    public string PizzaSize { get; set; }
    public string[] Toppings { get; set; }
}

public class Person
{
    public string Name { get; set; }
    public string Address { get; set; }
}

public class Order
{
    public string UserName { get; set; }
    public string UserAddress { get; set; }
    public List<Pizza> ChosenPizzas { get; set; }
}



