﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParatureSDK.ParaObjects.EntityReferences
{
    public class TicketReference: EntityReference<Ticket>
    {
        public Ticket Ticket
        {
            get { return base.Entity; }
            set { base.Entity = value; }
        }
    }
}
