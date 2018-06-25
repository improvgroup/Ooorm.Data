using Ooorm.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ooorm.Data
{
    public interface IDbItem
    {
        [Id]
        [Column(nameof(ID))]
        int ID { get; set; }
    }
}
