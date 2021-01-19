using System;
using System.IO;

namespace AdminOperation
{
    class CopyOperation : IOperation
    {
        public void Execute(string[] args)
        {
            File.Copy(args[1], args[2], true);
        }
    }
    class MoveOperation : IOperation
    {
        public void Execute(string[] args)
        {
            File.Move(args[1], args[2]);
        }
    }
}
