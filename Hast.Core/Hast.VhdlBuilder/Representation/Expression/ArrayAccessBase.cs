﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    public abstract class ArrayAccessBase : DataObjectBase
    {
        private IDataObject _arrayReference;
        public IDataObject ArrayReference
        {
            get { return _arrayReference; }
            set
            {
                _arrayReference = value;
                DataObjectKind = value.DataObjectKind;
                Name = value.Name;
            }
        }


        public override IDataObject ToReference() => this;
    }
}