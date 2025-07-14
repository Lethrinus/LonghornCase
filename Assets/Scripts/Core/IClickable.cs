
using Managers;

namespace Clickables {
    
    public interface IClickable
    {
        
        bool CanClickNow(GameState state);
    }
}