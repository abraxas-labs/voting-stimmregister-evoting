﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmregister.EVoting.Core.Configuration;

public class MachineConfig
{
    public string Name { get; set; } = Environment.MachineName;
}
