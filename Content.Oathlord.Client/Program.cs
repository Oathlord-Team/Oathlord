// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client;

namespace Content.Oathlord.Client
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ContentStart.Start(args);
        }
    }
};
