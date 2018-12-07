# ModbusTCP

This is a Modbus TCP implementation against .NET Standard 2.0.  It is meant to have as few dependencies outside of System as possible.

Development stage is early.  There are currently only two supported functions: Read Holding Registers (0x03) and Write Multiple Registers (0x10).  These were the first functions necessary for my own testing in another project.
