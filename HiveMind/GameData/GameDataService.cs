using SC2APIProtocol;

namespace HiveMind
{
    public class GameDataService : IGameDataService
    {
        public int GetAbilityId(int unitType)
        {
            // TODO: stick to uints
            return (int)Game.ResponseData.Units[unitType].AbilityId; // Reconsider static usage!! A StateManager would be nice...
        }
    }
}