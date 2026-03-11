using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nobeta.EUG.Interfaces
{
    /// <summary>
    /// 可更新的狀態
    /// </summary>
    public interface IUpdatable
    {
        void Update();
    }

    /// <summary>
    /// 可固定更新的狀態
    /// </summary>
    public interface IFixedUpdatable
    {
        void FixedUpdate();
    }
    public interface ILateUpdatable
    {
        void LateUpdate();
    }
}
