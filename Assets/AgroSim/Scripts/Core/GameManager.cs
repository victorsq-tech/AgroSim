using UnityEngine;

namespace AgroSim.Core
{
    public enum GameState { Garage, Traveling, Operating, Result, Returning }

    public sealed class GameManager : MonoBehaviour
    {
        public GameState CurrentState { get; private set; } = GameState.Garage;

        public void ChangeState(GameState state)
        {
            CurrentState = state;
        }
    }
}
