using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDCO.Combat.Integration
{
    public struct EntityRegistryEvent
    {
        public int entityId;
        public IEnemyCollisionRegistry collisionRegistry;
    }
}
