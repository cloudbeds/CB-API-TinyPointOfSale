using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

internal class PosItemSelectedEventArgs : EventArgs
{
    public readonly PosItem PosItem;
    public PosItemSelectedEventArgs(PosItem posItem) : base()
    {
        this.PosItem = posItem;
    }
}
