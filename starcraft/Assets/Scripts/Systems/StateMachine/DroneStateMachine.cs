using Core.Enums;
using Core.Events;
using Core.Interfaces;

namespace Systems.StateMachine
{
    /// <summary>
    /// State Machine специально для дрона
    /// Расширяет базовый StateMachine дополнительной функциональностью
    /// </summary>
    public class DroneStateMachine : StateMachine
    {
        private readonly IDrone _drone;

        /// <summary>
        /// Текущее состояние дрона (enum)
        /// </summary>
        public DroneState CurrentDroneState
        {
            get
            {
                if (CurrentState is IDroneState droneState)
                {
                    return droneState.StateType;
                }
                return DroneState.Idle;
            }
        }

        public DroneStateMachine(IDrone drone)
        {
            _drone = drone;
            OnStateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(IState previousState, IState newState)
        {
            DroneState previous = previousState is IDroneState prev ? prev.StateType : DroneState.Idle;
            DroneState current = newState is IDroneState curr ? curr.StateType : DroneState.Idle;
            
            var eventBus = Core.DI.DependencyHelper.GetEventBus();
            eventBus?.Publish(new DroneStateChangedEvent(_drone, previous, current));
        }
    }

    /// <summary>
    /// Интерфейс для состояний дрона с типом состояния
    /// </summary>
    public interface IDroneState : IState
    {
        DroneState StateType { get; }
    }
}

