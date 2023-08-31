using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SistemaInventario.Modelos.ViewModels
{
    public class ProductoVM
    {
        public Producto Producto { get; set; }
        public IEnumerable<Categoria>? CategoriaLista { get; set; }
        public IEnumerable<Marca>? MarcaLista { get; set; }
        public IEnumerable<Producto>? PadreLista { get; set; }
    }
}
