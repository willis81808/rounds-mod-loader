using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoundsModLoader
{
    public static class Utilities
    {
        public static void DestroyChildren(GameObject t)
        {
            while (t.transform.childCount > 0)
            {
                GameObject.DestroyImmediate(t.transform.GetChild(0).gameObject);
            }
        }
    }
}
