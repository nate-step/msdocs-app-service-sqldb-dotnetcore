using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    public class ParentPathMetadata
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }

        public ParentPathMetadata(string id, string name, string code)
        {
            Id = id;
            Name = name;
            Code = code;
        }
    }
}
