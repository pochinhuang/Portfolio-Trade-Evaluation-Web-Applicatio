using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;



namespace FM5353_HW05
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Build and run the application host
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddControllers();

                        // Add CORS policy
                        services.AddCors(options =>
                        {
                            options.AddPolicy("AllowAll", builder =>
                                builder.AllowAnyOrigin()
                                       .AllowAnyMethod()
                                       .AllowAnyHeader());
                        });

                        // Add Swagger services
                        services.AddEndpointsApiExplorer();
                        services.AddSwaggerGen();
                    })
                    .Configure((context, app) =>
                    {
                        // Inject IWebHostEnvironment
                        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

                        // Enable Swagger for development environment
                        if (environment.IsDevelopment())
                        {
                            app.UseSwagger();
                            app.UseSwaggerUI();
                        }

                        app.UseCors("AllowAll");

                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
                });
    }



    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=1126;Database=test;";
        public class Exchange
        {
            public string ExchangeId { get; set; }  // Exchange ID
            public string ExchangeName { get; set; }  // Exchange Name
            public string Location { get; set; }  // Location
            public string Country { get; set; }  // Country
            public string Currency { get; set; }  // Currency
        }


        // Get all exchanges
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Exchange> exchanges = new List<Exchange>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // all exchange data
                var cmd = new NpgsqlCommand("SELECT \"Exchange ID\", \"Exchange Name\", \"Location\", \"Country\", \"Currency\" FROM \"Exchange\"", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exchanges.Add(new Exchange
                        {
                            ExchangeId = reader.GetString(0),
                            ExchangeName = reader.GetString(1),
                            Location = reader.GetString(2),
                            Country = reader.GetString(3),
                            Currency = reader.GetString(4)
                        });
                    }
                }
            }
            return Ok(exchanges);
        }

        // Add a new exchange
        [HttpPost]
        public IActionResult Add([FromBody] Exchange exchange)
        {
            if (exchange == null)
                return BadRequest("Invalid data.");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(
                    "INSERT INTO \"Exchange\" (\"Exchange ID\", \"Exchange Name\", \"Location\", \"Country\", \"Currency\") " +
                    "VALUES (@ExchangeId, @ExchangeName, @Location, @Country, @Currency) RETURNING \"Exchange ID\"",
                    connection
                );
                cmd.Parameters.AddWithValue("ExchangeId", exchange.ExchangeId);
                cmd.Parameters.AddWithValue("ExchangeName", exchange.ExchangeName);
                cmd.Parameters.AddWithValue("Location", exchange.Location);
                cmd.Parameters.AddWithValue("Country", exchange.Country);
                cmd.Parameters.AddWithValue("Currency", exchange.Currency);

                // Retrieve the newly created Exchange ID
                exchange.ExchangeId = (string)cmd.ExecuteScalar();
            }

            return CreatedAtAction(nameof(GetAll), new { id = exchange.ExchangeId }, exchange);
        }

        // Delete an exchange by ID
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("DELETE FROM \"Exchange\" WHERE \"Exchange ID\" = @Id", connection);
                cmd.Parameters.AddWithValue("Id", id);
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    return Ok($"Record with ID {id} has been deleted.");
                else
                    return NotFound($"Record with ID {id} not found.");
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AssetController : ControllerBase
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=1126;Database=test;";

        // Asset class
        public class Asset
        {
            public string AssetType { get; set; }
            public string Explanation { get; set; }
            public string ExchangeId { get; set; }
        }

        // Get all assets
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Asset> assets = new List<Asset>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // 查詢所有 Asset 資料
                var cmd = new NpgsqlCommand("SELECT \"Asset\", \"Explanation\", \"Exchange ID\" FROM \"Asset\"", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        assets.Add(new Asset
                        {
                            AssetType = reader.GetString(0), 
                            Explanation = reader.GetString(1),
                            ExchangeId = reader.GetString(2)
                        });
                    }
                }
            }
            return Ok(assets);
        }
        
        // Add a new asset
        [HttpPost]
        public IActionResult Add([FromBody] Asset asset)
        {
            if (asset == null)
                return BadRequest("Invalid data.");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(
                    "INSERT INTO \"Asset\" (\"Asset\", \"Explanation\", \"Exchange ID\") " +
                    "VALUES (@AssetType, @Explanation, @ExchangeId) RETURNING \"Asset\"",
                    connection
                );
                cmd.Parameters.AddWithValue("AssetType", asset.AssetType);
                cmd.Parameters.AddWithValue("Explanation", asset.Explanation);
                cmd.Parameters.AddWithValue("ExchangeId", asset.ExchangeId);

                // Retrieve the newly created Asset
                asset.AssetType = (string)cmd.ExecuteScalar();
            }

            return CreatedAtAction(nameof(GetAll), new { assetType = asset.AssetType }, asset);
        }

        // Delete an asset by AssetType
        [HttpDelete("{assetType}")]
        public IActionResult Delete(string assetType)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("DELETE FROM \"Asset\" WHERE \"Asset\" = @AssetType", connection);
                cmd.Parameters.AddWithValue("AssetType", assetType);
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    return Ok($"Record with AssetType '{assetType}' has been deleted.");
                else
                    return NotFound($"Record with AssetType '{assetType}' not found.");
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class UnderlyingController : ControllerBase
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=1126;Database=test;";

        public class Underlying
        {
            public int ID { get; set; } 
            public string Name { get; set; }
            public string Ticker { get; set; }    
            public double Price { get; set; }    
            public string Asset { get; set; }    
            public string ExchangeId { get; set; } 
        }

        // Get all underlyings
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Underlying> underlyings = new List<Underlying>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // 查詢所有 Underlying 資料
                var cmd = new NpgsqlCommand("SELECT \"ID\", \"Name\", \"Ticker\", \"Price\", \"Asset\", \"Exchange ID\" FROM \"Underlying\"", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        underlyings.Add(new Underlying
                        {
                            ID = reader.GetInt32(0),   
                            Name = reader.GetString(1),       
                            Ticker = reader.GetString(2),     
                            Price = reader.GetDouble(3),     
                            Asset = reader.GetString(4),     
                            ExchangeId = reader.GetString(5)   
                        });
                    }
                }
            }
            return Ok(underlyings);
        }
            // Add a new underlying
        [HttpPost]
        public IActionResult Add([FromBody] Underlying underlying)
        {
            if (underlying == null)
                return BadRequest("Invalid data.");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // Insert all fields of Underlying into the database
                var cmd = new NpgsqlCommand("INSERT INTO \"Underlying\" (\"Name\", \"Ticker\", \"Price\", \"Asset\", \"Exchange ID\") " +
                                            "VALUES (@Name, @Ticker, @Price, @Asset, @ExchangeId) RETURNING \"ID\"", connection);
                cmd.Parameters.AddWithValue("Name", underlying.Name);
                cmd.Parameters.AddWithValue("Ticker", underlying.Ticker);
                cmd.Parameters.AddWithValue("Price", underlying.Price);
                cmd.Parameters.AddWithValue("Asset", underlying.Asset);
                cmd.Parameters.AddWithValue("ExchangeId", underlying.ExchangeId);

                // Retrieve the newly created ID
                underlying.ID = (int)cmd.ExecuteScalar();
            }

            return CreatedAtAction(nameof(GetAll), new { id = underlying.ID }, underlying);
        }

        // Delete an underlying by ID
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("DELETE FROM \"Underlying\" WHERE \"ID\" = @Id", connection);
                cmd.Parameters.AddWithValue("Id", id);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    return Ok($"Record with ID {id} has been deleted.");
                else
                    return NotFound($"Record with ID {id} not found.");
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class RateCurveController : ControllerBase
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=1126;Database=test;";

        public class RateCurve
        {
            public int Id { get; set; }               
            public string MaturityYear { get; set; }
            public double Maturity { get; set; }         
            public double? ZeroCouponBonds { get; set; }  
            public double? USTreasurys { get; set; }      
        }

        // Get all rate curve data
        [HttpGet]
        public IActionResult GetAll()
        {
            List<RateCurve> rateCurves = new List<RateCurve>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // 查詢所有 Rate Curve 資料
                var cmd = new NpgsqlCommand("SELECT \"ID\", \"Maturity (Year)\", \"Maturity\", \"Zero Coupon Bonds (%)\", \"U.S. Treasurys (%)\" FROM \"Rate Curve\"", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rateCurves.Add(new RateCurve
                        {
                            Id = reader.GetInt32(0),               
                            MaturityYear = reader.GetString(1),
                            Maturity = reader.GetDouble(2),             
                            ZeroCouponBonds = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                            USTreasurys = reader.IsDBNull(4) ? null : reader.GetDouble(4)     
                        });
                    }
                }
            }
            return Ok(rateCurves);
        }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class DerivativeController : ControllerBase
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=1126;Database=test;";

        public class Derivative
        {
            public int Id { get; set; }               
            public string ContractName { get; set; }  
            public string Type { get; set; }         
            public string Underlying { get; set; } 
            public double? Strike { get; set; }     
            public string CallPut { get; set; } 
            public double? Payout { get; set; } 
            public double? Barrier { get; set; }   
            public DateTime Expiration { get; set; } 
        }

        // Get all derivatives
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Derivative> derivatives = new List<Derivative>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // 查詢所有 Derivative 資料
                var cmd = new NpgsqlCommand("SELECT \"ID\", \"Contract Name\", \"Type\", \"Underlying\", \"Strike\", \"Call/Put\", \"Payout\", \"Barrier\", \"Expiration\" FROM \"Derivative\"", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        derivatives.Add(new Derivative
                        {
                            Id = reader.GetInt32(0),      
                            ContractName = reader.GetString(1), 
                            Type = reader.GetString(2),              
                            Underlying = reader.GetString(3), 
                            Strike = reader.IsDBNull(4) ? null : reader.GetDouble(4), 
                            CallPut = reader.GetString(5),   
                            Payout = reader.IsDBNull(6) ? null : reader.GetDouble(6), 
                            Barrier = reader.IsDBNull(7) ? null : reader.GetDouble(7), 
                            Expiration = reader.GetDateTime(8)
                        });
                    }
                }
            }
            return Ok(derivatives);
        }


        // Add a new derivative
        [HttpPost]
        public IActionResult Add(Derivative derivative)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(
                    "INSERT INTO \"Derivative\" (\"Contract Name\", \"Type\", \"Underlying\", \"Strike\", \"Call/Put\", \"Payout\", \"Barrier\", \"Expiration\") VALUES (@ContractName, @Type, @Underlying, @Strike, @CallPut, @Payout, @Barrier, @Expiration) RETURNING \"ID\"",
                    connection);

                cmd.Parameters.AddWithValue("ContractName", derivative.ContractName);
                cmd.Parameters.AddWithValue("Type", derivative.Type);
                cmd.Parameters.AddWithValue("Underlying", derivative.Underlying);
                cmd.Parameters.AddWithValue("Strike", derivative.Strike ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("CallPut", derivative.CallPut);
                cmd.Parameters.AddWithValue("Payout", derivative.Payout ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("Barrier", derivative.Barrier ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("Expiration", derivative.Expiration);

                int newId = (int)cmd.ExecuteScalar();
                return CreatedAtAction(nameof(GetAll), new { id = newId }, derivative);
            }
        }

        // Delete a derivative by ID
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("DELETE FROM \"Derivative\" WHERE \"ID\" = @Id", connection);
                cmd.Parameters.AddWithValue("Id", id);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok($"Derivative with ID {id} has been deleted.");
                }
                else
                {
                    return NotFound($"Derivative with ID {id} not found.");
                }
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class TradeController : ControllerBase
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=1126;Database=test;";

        public class Trade
        {
            public int Id { get; set; }
            public string ContractName { get; set; }
            public string Type { get; set; }
            public string Underlying { get; set; }
            public double? Strike { get; set; }
            public string CallPut { get; set; }
            public double? Payout { get; set; }
            public double? Barrier { get; set; }
            public DateTime Expiration { get; set; }
            public double? StockPrice { get; set; }
            public int? TradeQuantity { get; set; }
            public double? TradePrice { get; set; }
            public double? RiskFreeRate { get; set; }
            public DateTime TradeDate { get; set; }  
        }

        // Get all trades
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Trade> trades = new List<Trade>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(
                    "SELECT \"ID\", \"Contract Name\", \"Type\", \"Underlying\", \"Strike\", \"Call/Put\", \"Payout\", \"Barrier\", \"Expiration\", \"StockPrice\", \"TradeQuantity\", \"TradePrice\", \"RiskFreeRate\", \"TradeDate\" FROM \"Trade\"",
                    connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trades.Add(new Trade
                        {
                            Id = reader.GetInt32(0),
                            ContractName = reader.GetString(1),
                            Type = reader.GetString(2),
                            Underlying = reader.GetString(3),
                            Strike = reader.IsDBNull(4) ? null : reader.GetDouble(4),
                            CallPut = reader.GetString(5),
                            Payout = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                            Barrier = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                            Expiration = reader.GetDateTime(8),
                            StockPrice = reader.IsDBNull(9) ? null : reader.GetDouble(9),
                            TradeQuantity = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                            TradePrice = reader.IsDBNull(11) ? null : reader.GetDouble(11),
                            RiskFreeRate = reader.IsDBNull(12) ? null : reader.GetDouble(12),
                            TradeDate = reader.GetDateTime(13)
                        });
                    }
                }
            }
            return Ok(trades);
        }

        // Add a new trade
        [HttpPost]
        public IActionResult Add([FromBody] Trade trade)
        {
            if (trade == null)
                return BadRequest("Invalid trade data.");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(
                    @"INSERT INTO ""Trade"" 
                    (""Contract Name"", ""Type"", ""Underlying"", ""Strike"", ""Call/Put"", ""Payout"", ""Barrier"", ""Expiration"", 
                    ""StockPrice"", ""TradeQuantity"", ""TradePrice"", ""RiskFreeRate"", ""TradeDate"") 
                    VALUES 
                    (@ContractName, @Type, @Underlying, @Strike, @CallPut, @Payout, @Barrier, @Expiration, 
                    @StockPrice, @TradeQuantity, @TradePrice, @RiskFreeRate, @TradeDate) 
                    RETURNING ""ID""",
                    connection);

                // Map parameters
                cmd.Parameters.AddWithValue("ContractName", trade.ContractName);
                cmd.Parameters.AddWithValue("Type", trade.Type);
                cmd.Parameters.AddWithValue("Underlying", trade.Underlying);
                cmd.Parameters.AddWithValue("Strike", trade.Strike ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("CallPut", trade.CallPut);
                cmd.Parameters.AddWithValue("Payout", trade.Payout ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("Barrier", trade.Barrier ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("Expiration", trade.Expiration);
                cmd.Parameters.AddWithValue("StockPrice", trade.StockPrice ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("TradeQuantity", trade.TradeQuantity ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("TradePrice", trade.TradePrice ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("RiskFreeRate", trade.RiskFreeRate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("TradeDate", trade.TradeDate);

                try
                {
                    // Execute and retrieve the newly created ID
                    int newId = (int)cmd.ExecuteScalar();
                    trade.Id = newId;
                    return CreatedAtAction(nameof(GetAll), new { id = newId }, trade);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while inserting trade: " + ex.Message);
                    return StatusCode(500, "Internal server error.");
                }
            }
        }
        // Delete a trade by ID
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                try
                {

                    var cmd = new NpgsqlCommand(
                        "DELETE FROM \"Trade\" WHERE \"ID\" = @Id",
                        connection);


                    cmd.Parameters.AddWithValue("Id", id);


                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok($"Trade with ID {id} deleted successfully.");
                    }
                    else
                    {
                        return NotFound($"No trade found with ID {id}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while deleting trade: " + ex.Message);
                    return StatusCode(500, "Internal server error.");
                }
            }
        }
    }



    [Route("api/[controller]")]
    [ApiController]
    public class MonteCarloController : ControllerBase
    {
        [HttpPost("simulate")]
        public ActionResult<MonteCarloResult> Simulate([FromBody] MonteCarloInput input)
        {
            // Random generateor
            NormalGenerator generator = new NormalGenerator();
            double[,] normal = generator.Generate(10000, 1000);
            double sigma = 0.2;

            double T = (input.EndDate - input.StartDate).TotalDays / 365;

            // run montecarlo
            MonteCarlo monteCarlo = new MonteCarlo();
            double[] Simulate = monteCarlo.Simulation(
                input.StockPrice, 
                input.StrikePrice, 
                input.RiskFreeRate,
                sigma,
                T, 
                normal, 
                input.OptionClass,
                input.OptionType,
                input.P, 
                input.H
                );


            // calculate price and standard error
            Results result = new Results();
            double Price = result.Price(Simulate, input.RiskFreeRate, T);
            double SE = result.StandardError(Simulate);

            // calculate greeks
            Greeks greeks = new Greeks(
                input.StockPrice, input.StrikePrice, input.RiskFreeRate, sigma, T, normal, 
                input.OptionClass, input.OptionType, input.P, input.H);

            double Delta = greeks.Delta();
            double Gamma = greeks.Gamma();
            double Vega = greeks.Vega();
            double Theta = greeks.Theta();
            double Rho = greeks.Rho();

            return Ok(new MonteCarloResult {
                                            ContractName = input.ContractName,
                                            Quantity = input.Quantity,
                                            TradePrice = input.TradePrice,
                                            Price = Price, 
                                            SE = SE,
                                            Delta = Delta,
                                            Gamma = Gamma,
                                            Vega = Vega,
                                            Theta = Theta,
                                            Rho = Rho,
                                            });
        }
    }

    public class MonteCarloInput
    {
        public string ContractName { get; set; }
        public int Quantity { get; set; }
        public double TradePrice { get; set; }
        public double StockPrice { get; set; }
        public double StrikePrice { get; set; } = 0;
        public double RiskFreeRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string OptionClass { get; set; }
        public string OptionType { get; set; }
        public double P { get; set; } = 0;
        public double H { get; set; } = 0;
    }


    public class MonteCarloResult
    {
        public string ContractName { get; set; }
        public int Quantity { get; set; }
        public double TradePrice { get; set; }
        public double Price { get; set; }
        public double SE { get; set; }
        public double Delta { get; set; }
        public double Gamma { get; set; }
        public double Vega { get; set; }
        public double Theta { get; set; }
        public double Rho { get; set; }
    }


    // Payoff Function Class
    class Payoff
    {
        public double func(double ST, double K, string OptionType, string OptionClass, 
        double[] STs = null, double? P = 0, double? H = 0)
        {
            string optionTypeLower = OptionType.ToLower();
            string optionClassLower = OptionClass.ToLower();

            // european option
            if (optionClassLower == "european")
            {
                if (optionTypeLower == "call")
                {
                    return Math.Max(ST - K, 0);
                }
                else
                {
                    return Math.Max(K - ST, 0);
                }                
            }
            // asian option
            else if (optionClassLower == "asian")
            {
                double STMean = STs.Average();

                if (optionTypeLower == "call")
                {
                    return Math.Max(STMean - K, 0);
                }
                else
                {
                    return Math.Max(K - STMean, 0);
                }                  
            }
            // digital option
            else if (optionClassLower == "ditital")
            {
                if (optionTypeLower == "call")
                {
                    if (ST > K)
                    {
                        return P ?? 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (ST < K)
                    {
                        return P ?? 0;
                    }
                    else
                    {
                        return 0; 
                    }                    
                }
            }
            // barrier option

            // Up-and-In
            else if (optionClassLower == "up-and-in")
            {
                bool TouchedBarrier = STs.Any(s => s >= H);

                if (TouchedBarrier)
                {
                    if (optionTypeLower == "call")
                    {
                        return Math.Max(ST - K, 0);
                    }
                    else
                    {
                        return Math.Max(K - ST, 0);
                    }
                }
                    
                else
                {
                    return 0;
                }
            }
            // Down-and-In
            else if (optionClassLower == "down-and-in")
            {
                bool TouchedBarrier = STs.Any(s => s <= H);

                if (TouchedBarrier)
                {
                    if (optionTypeLower == "call")
                    {
                        return Math.Max(ST - K, 0);
                    }
                    else
                    {
                        return Math.Max(K - ST, 0);
                    }
                }
                    
                else
                {
                    return 0;
                }
            }

            // Up-and-Out
            else if (optionClassLower == "up-and-out")
            {
                bool TouchedBarrier = STs.Any(s => s >= H);

                if (TouchedBarrier)
                {
                    return 0;
                }
                    
                else
                {
                    if (optionTypeLower == "call")
                    {
                        return Math.Max(ST - K, 0);
                    }
                    else
                    {
                        return Math.Max(K - ST, 0);
                    }
                }
            }

            // Down-and-Out
            else if (optionClassLower == "down-and-out")
            {
                bool TouchedBarrier = STs.Any(s => s <= H);

                if (TouchedBarrier)
                {
                    return 0;
                }
                    
                else
                {
                    if (optionTypeLower == "call")
                    {
                        return Math.Max(ST - K, 0);
                    }
                    else
                    {
                        return Math.Max(K - ST, 0);
                    }
                }
            }
        
            // lookback option

            // fixed strike
            else if (optionClassLower == "lookback")
            {
                if (optionTypeLower == "call")
                {
                    return Math.Max(STs.Max() - K, 0);
                }
                else
                {
                    return Math.Max(K - STs.Min(), 0);
                }                    
            }
            
            // range option
            else if (optionClassLower == "range")
            {
                return STs.Max() - STs.Min();
            }
                
            return 0;
        }
    }

    // normally distributed random numbers using the Box-Muller
        class NormalGenerator
    {
        private Random random = new Random();
        // Generate a matrix of standard normal random variables
        public double[,] Generate(int N, int steps)
        {
            double[,] normalMatrix = new double[N, steps];

            // number of simulation
            for (int i = 0; i < N; i++)
            {
                // number of steps
                for (int j = 0; j < steps; j++)
                {
                    // Use Box-Muller to generate normal random numbers
                    double u1 = random.NextDouble();
                    double u2 = random.NextDouble();

                    double z1 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                    // Box-Muller gives two independent standard normal variables
                    normalMatrix[i, j] = z1;
                }
            }

            return normalMatrix;
        }
    }

    // Monte Carlo Class
    class MonteCarlo
    {
        public double[]
        Simulation( double S,
                    double K, 
                    double r,
                    double sigma,
                    double T,
                    double[,] normal,
                    string OptionClass,
                    string OptionType,
                    double? P = 0,
                    double? H = 0
                    )
        
        {
            // Antithetic
            Payoff payoff = new Payoff();
            
            double dt = T / 1000.0;

            double[] Payoffs = new double[10000];

            int coreCount = Environment.ProcessorCount;

            Parallel.For(0, 10000, new ParallelOptions { MaxDegreeOfParallelism = coreCount }, i =>
            {
                double ST_OG = S;
                double ST_ANTI = S;
                double[] STs_OG = new double[1000];
                double[] STs_ANTI = new double[1000];

                // Generate original and opposite paths
                for (int j = 0; j < 1000; j++)
                {
                    ST_OG *= Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * dt + sigma * Math.Sqrt(dt) * normal[i, j]);
                    ST_ANTI *= Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * dt - sigma * Math.Sqrt(dt) * normal[i, j]);

                    // ST_OG *= Math.Exp((r - 0.5 * Math.Pow(0.2, 2)) * dt + 0.2 * Math.Sqrt(dt) * normal[i, j]);
                    // ST_ANTI *= Math.Exp((r - 0.5 * Math.Pow(0.2, 2)) * dt - 0.2 * Math.Sqrt(dt) * normal[i, j]);

                    STs_OG[j] = ST_OG;
                    STs_ANTI[j] = ST_ANTI;
                }

                // Average
                Payoffs[i] = 0.5 * (payoff.func(ST_OG, K, OptionType, OptionClass, STs_OG, P, H) + 
                payoff.func(ST_ANTI, K, OptionType, OptionClass, STs_ANTI, P, H));

            });
            return Payoffs;
        }
    }

    // Pass the payoffs here to calculate Prices and Standard Errors
    class Results
    {
        public double
        Price(double[] Payoffs, double r, double T)
        {
            double Price = Payoffs.Average() * Math.Exp(-r * T);

            return Price;
        }

        public double
        StandardError(double[] Payoffs)
        {
            double Mean = Payoffs.Average();
            double SumSquares = Payoffs.Select(val => (val - Mean) * (val - Mean)).Sum();
            double StandardDev = Math.Sqrt(SumSquares / (Payoffs.Length - 1));
            double StandardError = StandardDev / Math.Sqrt(Payoffs.Length);

            return StandardError;
        }
    }
    class Greeks
    {
        private double S;
        private double K;
        private double r;
        private double sigma;
        private double T;
        private double[,] normal;
        Results results = new Results();
        MonteCarlo montecarlo = new MonteCarlo();
        private string OptionClass;
        private string OptionType;
        private double? P;
        private double? H;

        public Greeks(double S, double K, double r, double sigma, double T, double[,] normal, string OptionClass, string OptionType, 
        double? P = null, double? H = null)
        {
            this.S = S;
            this.K = K;
            this.r = r;
            this.sigma = sigma;
            this.T = T;
            this.normal = normal;
            this.OptionClass = OptionClass;
            this.OptionType = OptionType;
            this.P = P;
            this.H = H;           
        }

        public double Delta()
        {
            // ΔS
            double DeltaS = S * 0.01;

            // S + ΔS and S - ΔS for call and put

            double[] PlusDelta = montecarlo.Simulation(S * 1.01, K, r, sigma, T, normal, OptionClass, OptionType, P, H);
            double[] MinusDelta = montecarlo.Simulation(S * 0.99, K, r, sigma, T, normal, OptionClass, OptionType, P, H);

            double PlusDelta_Price = results.Price(PlusDelta, r, T);
            double MinusDelta_Price = results.Price(MinusDelta, r, T);

            // delta call and put
            double Delta = (PlusDelta_Price - MinusDelta_Price) / (2 * DeltaS);
            

            return Delta;
        }

        public double Gamma()
        {
            // ΔS
            double DeltaS = S * 0.01;

            double[] PlusDelta = montecarlo.Simulation(S * 1.01, K, r, sigma, T, normal, OptionClass, OptionType, P, H);
            double[] MinusDelta = montecarlo.Simulation(S * 0.99, K, r, sigma, T, normal, OptionClass, OptionType, P, H);
            double[] Base = montecarlo.Simulation(S, K, r, sigma, T, normal, OptionClass, OptionType, P, H);

            double BasePrice = results.Price(Base, r, T);
            double PlusPrice = results.Price(PlusDelta, r, T);
            double MinusPrice = results.Price(MinusDelta, r, T);

            double Gamma = (PlusPrice - (2 * BasePrice) + MinusPrice) / Math.Pow(DeltaS, 2);

            return Gamma;
        }


        public double Vega()
        {
            // ΔSigma
            double DeltaSigma = sigma * 0.01;


            double[] PlusSigma = montecarlo.Simulation(S, K, r , sigma * 1.01, T, normal, OptionClass, OptionType, P, H);
            double[] MinusSigma = montecarlo.Simulation(S, K, r, sigma * 0.99, T, normal, OptionClass, OptionType, P, H);


            double PlusPrice = results.Price(PlusSigma, r, T);
            double MinusPrice = results.Price(MinusSigma, r, T);


            double Vega = (PlusPrice - MinusPrice) / (2 * DeltaSigma);

            return Vega;
        }
        public double Theta()
        {
            // ΔT
            double DeltaT = T * 0.01;


            double[] PlusTime = montecarlo.Simulation(S, K, r, sigma, T * 1.01, normal, OptionClass, OptionType, P, H);
            double[] BaseTime = montecarlo.Simulation(S, K, r, sigma, T, normal, OptionClass, OptionType, P, H);


            double PlusPrice = results.Price(PlusTime, r, T * 1.01);
            double BasePrice = results.Price(BaseTime, r, T);


            double Theta = (PlusPrice - BasePrice) / DeltaT;

            return Theta;
        }

        public double Rho()
        {
            // Δr
            double DeltaR = r * 0.01;


            double[] PlusRate = montecarlo.Simulation(S, K, r * 1.01, sigma, T, normal, OptionClass, OptionType, P, H);
            double[] MinusRate = montecarlo.Simulation(S, K, r * 0.99, sigma, T, normal, OptionClass, OptionType, P, H);


            double PlusPrice = results.Price(PlusRate, r * 1.01, T);
            double MinusPrice = results.Price(MinusRate, r * 0.99, T);


            double Rho = (PlusPrice - MinusPrice) / (2 * DeltaR);

            return Rho;
        }
    }
}