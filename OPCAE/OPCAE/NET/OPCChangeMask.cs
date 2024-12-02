namespace OPCAE.OPCAE.NET
{
    using System;

    public enum OPCChangeMask
    {
        OPC_CHANGE_ACTIVE_STATE = 1,
        OPC_CHANGE_ACK_STATE = 2,
        OPC_CHANGE_ENABLE_STATE = 4,
        OPC_CHANGE_QUALITY = 8,
        OPC_CHANGE_SEVERITY = 0x10,
        OPC_CHANGE_SUBCONDITION = 0x20,
        OPC_CHANGE_MESSAGE = 0x40,
        OPC_CHANGE_ATTRIBUTE = 0x80
    }
}

