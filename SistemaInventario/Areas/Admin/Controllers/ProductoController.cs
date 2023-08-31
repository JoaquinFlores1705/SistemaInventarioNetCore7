using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;
using System.Net;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductoController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IWebHostEnvironment _webHostEnviroment;

        public ProductoController(IUnidadTrabajo unidadTrabajo, IWebHostEnvironment webHostEnviroment)
        {
            _unidadTrabajo = unidadTrabajo;
            _webHostEnviroment = webHostEnviroment;

        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Upsert(int? id)
        {
            ProductoVM productoVM = new ProductoVM()
            {
                Producto = new Producto(),
                CategoriaLista = await _unidadTrabajo.Categoria.ObtenerTodos((c => c.Estado == true)),
                MarcaLista = await _unidadTrabajo.Marca.ObtenerTodos((m => m.Estado == true)),
                PadreLista = await _unidadTrabajo.Producto.ObtenerTodos((p => p.Estado == true)),
            };

            if(id == null)
            {
                productoVM.Producto.Estado = true;
                return View(productoVM);
            }
            else
            {
                productoVM.Producto = await _unidadTrabajo.Producto.Obtener(id.GetValueOrDefault());
                if(productoVM.Producto == null)
                {
                    return NotFound();
                }
                return View(productoVM);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductoVM productoVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnviroment.WebRootPath;

                if(productoVM.Producto.Id == 0)
                {
                    string upload = webRootPath + DS.ImagenRuta;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using(var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productoVM.Producto.ImagenUrl = fileName + extension;
                    await _unidadTrabajo.Producto.Agregar(productoVM.Producto);
                    
                }
                else
                {
                    var objProducto = await _unidadTrabajo.Producto.ObtenerPrimero((p => p.Id == productoVM.Producto.Id), isTracking: false);

                    if (files.Count > 0)
                    {
                        string upload = webRootPath + DS.ImagenRuta;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var anteriorFile = Path.Combine(upload, objProducto.ImagenUrl);

                        if (System.IO.File.Exists(anteriorFile))
                        {
                            System.IO.File.Delete(anteriorFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        productoVM.Producto.ImagenUrl = fileName + extension;

                    }
                    else
                        productoVM.Producto.ImagenUrl = objProducto.ImagenUrl;

                    _unidadTrabajo.Producto.Actualizar(productoVM.Producto);
                }
                TempData[DS.exitosa] = "Transaccion exitosa";
                await _unidadTrabajo.Guardar();
                return RedirectToAction(nameof(Index));
            }

            productoVM.CategoriaLista = await _unidadTrabajo.Categoria.ObtenerTodos((c => c.Estado == true));
            productoVM.MarcaLista = await _unidadTrabajo.Marca.ObtenerTodos((m => m.Estado == true));
            productoVM.PadreLista = await _unidadTrabajo.Producto.ObtenerTodos((p => p.Estado == true));
            return View(productoVM);
        }

        

        #region API

        [HttpGet]
        public async Task<IActionResult> obtenerTodos()
        {
            var todos = _unidadTrabajo.Producto.ObtenerTodos(incluirPropiedades: "Categoria,Marca");
            return Json(new { data = todos.Result });

        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var productoDb = await _unidadTrabajo.Producto.Obtener(id);
            if (productoDb == null)
                return Json(new { success = false, message = "Error al borrar producto" });

            string upload = _webHostEnviroment.WebRootPath + DS.ImagenRuta;
            var anteriorFile = Path.Combine(upload, productoDb.ImagenUrl!);
            if (System.IO.File.Exists(anteriorFile))
            {
                System.IO.File.Delete(anteriorFile);
            }


            _unidadTrabajo.Producto.Remover(productoDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Producto borrada correctamente" });
        }

        [ActionName("ValidarSerie")]
        [HttpGet]
        public async Task<IActionResult> ValidarSerie(string serie, int id = 0)
        {
            bool valor = false;
            var lista = await _unidadTrabajo.Producto.ObtenerTodos((b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim()));
            valor = id == 0 ? lista.Any() : lista.Any(l => l.Id != id);
            return Json(new { data = valor });
        }


        #endregion
    }
}
