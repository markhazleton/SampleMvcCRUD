using System.ComponentModel.DataAnnotations;

namespace Mwh.Sample.BlazorCRUD.Entities
{
    public class Article
    {
        [Key]
        public int ID { get; set; }
        public string Title { get; set; }
    }
}
