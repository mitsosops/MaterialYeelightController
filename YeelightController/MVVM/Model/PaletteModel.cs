﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YeelightController.MVVM.Model
{
    internal class PaletteModel
    {
        private string _primaryColor;

        public string PrimaryColor
        {
            get { return _primaryColor; }
            set
            {
                 
                    if (value != _primaryColor)
                    {
                        _primaryColor = value;
                    }
                               
            }
        }

        private string _secondaryColor;

        public string SecondaryColor
        {
            get { return _secondaryColor; }
            set
            {
                {
                    if (value != _secondaryColor)
                    {
                        _secondaryColor = value;
                    }
                }
            }
        }

    }
}
