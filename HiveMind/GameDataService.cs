using SC2APIProtocol;

namespace HiveMind
{
    public class GameDataService : IGameDataService
    {
        public int GetAbilityId(int unitType)
        {
            return (int)Game.ResponseData.Units[unitType].AbilityId; // Reconsider static usage!! A StateManager would be nice...
        }
    }
}