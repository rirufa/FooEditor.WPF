using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdminOperation
{
    interface IOperation
    {
        /// <summary>
        /// 操作を実行する
        /// </summary>
        /// <param name="args">パラメーターとなる文字列の配列</param>
        void Execute(string[] args);
    }

    class OperationFactory
    {
        public static IOperation Create(string operation)
        {
            switch (operation)
            {
                case "copy":
                    return new CopyOperation();
                case "move":
                    return new MoveOperation();
            }
            throw new ArgumentOutOfRangeException();
        }
    }
}
