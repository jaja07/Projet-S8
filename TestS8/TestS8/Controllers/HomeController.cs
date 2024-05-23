using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using TestS8.Data;
using TestS8.Models;

namespace TestS8.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            if (User.IsInRole("Lambda"))
            {
                return RedirectToAction("Guest", "Home");
            }
            else if (User.IsInRole("Admin"))
            {
                return RedirectToAction("IndexAdmin", "Home");
            }
            else if (User.IsInRole("Expert"))
            {
                return RedirectToAction("Admin", "Home");
            }
            else
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var model = new Methods
            {

            };
            return View(model);
        }

        public async Task<IActionResult> Upload(IFormFile file)  // Ceci est une méthode d'action; chaque méthode d'action correspond à un URL du site
        {
            // Chemin de destination pour enregistrer le fichier dans le sous-répertoire spécifié
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Python", "data.zip");

            // Enregistrement du fichier sur le serveur
            using (var stream = new FileStream(filePath, FileMode.Create)) // Créer un flux de fichier pour enregistrer le fichier
            {
                await file.CopyToAsync(stream); // Copier les données du fichier téléchargé dans le flux de fichier de destination
            }
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync("http://localhost:5000/pretraitement", null);

            }
            return View("Index");
        }

        public async Task<IActionResult> Valider(Methods model)  // Ceci est une méthode d'action; chaque méthode d'action correspond à un URL du site
        {
            var user = await _userManager.GetUserAsync(User);
            var connexion = await _context.Connexion
                              .OrderByDescending(c => c.ConnexionID)
                              .FirstOrDefaultAsync();

            // Gestion de l'historique
            // Table simulation
            Simulation simulation = new Simulation
            {
                Date = DateTime.Now, // Date courante
                ConnexionID = connexion.ConnexionID
            };
            _context.Simulation.Add(simulation);
            await _context.SaveChangesAsync();
            var all_simulation = await _context.Simulation.ToListAsync();
            int lenght = all_simulation.Count();
            int simulationId = all_simulation[lenght - 1].SimulationID;
            //FormatException: Input string was not in a correct format
            if (ModelState.IsValid)
            {
                if (model.Analytique)
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsync("http://localhost:5000/analytical", null);
                        var result = await response.Content.ReadAsStringAsync();
                        ViewBag.Analytique = result;
                    }
                    // Modèles
                    var modele = new Modele
                    {
                        Nom = "Analytique",
                        Accuracy = ExtractNumber(ViewBag.Analytique),
                        Accuracy_cross = 1,
                        Hyperparametre = " ",
                        Duree_simul = ExtractTime(ViewBag.Analytique),
                        SimulationID = simulationId
                    };
                    _context.Modele.Add(modele);
                    await _context.SaveChangesAsync();
                    int modeleId = modele.ModeleID;
                }
                if (model.KNN)
                {
                    // Préparation de la requête http à envoyer au service python (flask_api_test.py)
                    using (var client = new HttpClient())
                    {
                        var requestData = new
                        {
                            Parameter1 = model.n_neighbors,
                            Parameter2 = model.weights,
                            Parameter3 = model.algorithm,
                            Parameter4 = model.p_knn
                        };
                        var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("http://localhost:5000/knn", content);
                        var result = await response.Content.ReadAsStringAsync();
                        ViewBag.KNN = result;
                        // Modèles
                        var modele = new Modele
                        {
                            Nom = "KNN",
                            Accuracy = ExtractNumber(ViewBag.KNN),
                            Accuracy_cross = 1,
                            Hyperparametre = @"{n_neighbors:" + requestData.Parameter1.ToString() + "," +
                                                "weights:" + requestData.Parameter2.ToString() + "," +
                                                "algorithm:" + requestData.Parameter3.ToString() + "," +
                                                "p_knn:" + requestData.Parameter4.ToString() + "," + "}",
                            Duree_simul = ExtractTime(ViewBag.KNN),
                            SimulationID = simulationId
                        };
                        _context.Modele.Add(modele);
                        await _context.SaveChangesAsync();
                        int modeleId = modele.ModeleID;
                    }

                }
                if (model.RandomForest)
                {
                    // Préparation de la requête http à envoyer au service python (flask_api_test.py)
                    using (var client = new HttpClient())
                    {
                        var requestData = new
                        {
                            Parameter1 = model.n_estimators,
                            Parameter2 = model.max_depth,
                            Parameter3 = model.min_samples_split,
                            Parameter4 = model.min_samples_leaf,
                            Parameter5 = model.bootstrap
                        };
                        var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("http://localhost:5000/randomforest", content);
                        var result = await response.Content.ReadAsStringAsync();
                        ViewBag.Randomforest = result;

                        // Modèles
                        var modele = new Modele
                        {
                            Nom = "Random Forest",
                            Accuracy = ExtractNumber(ViewBag.Randomforest),
                            Accuracy_cross = 1,
                            Hyperparametre = @"{n_estimators:" + requestData.Parameter1.ToString() + "," +
                                                "max_depth:" + requestData.Parameter2.ToString() + "," +
                                                "min_samples_split:" + requestData.Parameter3.ToString() + "," +
                                                "min_samples_leaf:" + requestData.Parameter4.ToString() + "," +
                                                "bootstrap:" + requestData.Parameter5.ToString() + "}",
                            Duree_simul = ExtractTime(ViewBag.KNN),
                            SimulationID = simulationId
                        };
                        _context.Modele.Add(modele);
                        await _context.SaveChangesAsync();
                        int modeleId = modele.ModeleID;
                    }
                }
                if (model.SVM)
                {
                    // Préparation de la requête http à envoyer au service python (flask_api_test.py)
                    using (var client = new HttpClient())
                    {
                        var requestData = new
                        {
                            Parameter1 = model.kernel,
                            Parameter2 = model.C,
                            Parameter3 = model.probability,
                            Parameter4 = model.tol
                        };
                        var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("http://localhost:5000/svm", content);
                        var result = await response.Content.ReadAsStringAsync();
                        ViewBag.SVM = result;
                        // Modèles
                        var modele = new Modele
                        {
                            Nom = "SVM",
                            Accuracy = ExtractNumber(ViewBag.SVM),
                            Accuracy_cross = 1,
                            Hyperparametre = @"{kernel:" + requestData.Parameter1.ToString() + "," +
                                                "C:" + requestData.Parameter2.ToString() + "," +
                                                "probability:" + requestData.Parameter3.ToString() + "," +
                                                "tol:" + requestData.Parameter4.ToString() + "," + "}",

                            Duree_simul = ExtractTime(ViewBag.SVM),
                            SimulationID = simulationId
                        };
                        _context.Modele.Add(modele);
                        await _context.SaveChangesAsync();
                        int modeleId = modele.ModeleID;
                    }
                }
            }
            await _context.SaveChangesAsync();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Python", "data.zip");
            var filePath2 = Path.Combine(Directory.GetCurrentDirectory(), "Python", "data_anonymous");
            try
            {
                // Suppression du fichier
                System.IO.File.Delete(filePath);
                //System.IO.File.Delete(filePath2);
                Console.WriteLine("Fichier supprimé avec succès.");
            }
            catch (IOException ioExp)
            {
                Console.WriteLine("Erreur lors de la suppression du fichier: " + ioExp.Message);
            }
            catch (UnauthorizedAccessException unAuthExp)
            {
                Console.WriteLine("Accès refusé: " + unAuthExp.Message);
            }
            return View("Result");
        }

        public async Task<IActionResult> ValiderGuest(Methods model)  // Ceci est une méthode d'action; chaque méthode d'action correspond à un URL du site
        {
            
            return View("Guest");
        }

        public async Task<IActionResult> Relancer(int? id)
        {
            if (id == null || _context.Parametres == null)
            {
                return NotFound();
            }
            var modele = await _context.Modele.FindAsync(id);
            var parametres = await _context.Parametres
                .Include(m => m.Modeles)
                .Where(m => m.Modeles.ModeleID == id) // Filter by SimulationID
                .ToListAsync();

            if (parametres.Count == 0)
            {
                return NotFound();
            }

            if(modele.Nom == "Analytique")
            {

            }
            if (modele.Nom == "Random Forest")
            {

            }
            if (modele.Nom == "KNN")
            {

            }
            if (modele.Nom == "SVM")
            {

            }
            return View("Result");
        }
        float ExtractNumber(string result)
        {
            if (!string.IsNullOrEmpty(result))
            {

                dynamic jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                if (jsonResult != null && jsonResult.accuracy != null)
                {
                    //  accuracy
                    return (float)jsonResult.accuracy;
                }
            }
            return 0;
        }
        float ExtractTime(string result)
        {
            if (!string.IsNullOrEmpty(result))
            {

                dynamic jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                if (jsonResult != null && jsonResult.time != null)
                {
                    //  accuracy
                    return (float)jsonResult.time;
                }
            }
            return 0;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult IndexAdmin()
        {
            return View("Index");
        }

        [Authorize(Roles = "Lambda")]
        public IActionResult Guest()
        {
            return View();
        }
        [Authorize(Roles = "Expert")]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Login()
        {
            var histories = await _context.Connexion.ToListAsync();
            return View("View/Connexion/Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
