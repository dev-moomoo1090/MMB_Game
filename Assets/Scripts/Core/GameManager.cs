using UnityEngine;

namespace MMBGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private BoardManager boardManager;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private PoliticsManager politicsManager;
        [SerializeField] private MilitaryManager militaryManager;
        [SerializeField] private PoliticalManager politicalManager;
        [SerializeField] private RebellionSystem rebellionSystem;
        [SerializeField] private KingStateEffectApplier kingStateEffectApplier;
        [SerializeField] private HonorPiecePassiveSystem honorPiecePassiveSystem;
        [SerializeField] private RegimeActionButtons regimeActionButtons;
        [SerializeField] private AutonomousMovement autonomousMovement;
        [SerializeField] private StockfishBridge stockfishBridge;
        public BoardManager BoardManager => boardManager;
        public TurnManager TurnManager => turnManager;
        public PoliticsManager PoliticsManager => politicsManager;
        public MilitaryManager MilitaryManager => militaryManager;
        public PoliticalManager PoliticalManager => politicalManager;
        public RebellionSystem RebellionSystem => rebellionSystem;
        public KingStateEffectApplier KingStateEffectApplier => kingStateEffectApplier;
        public HonorPiecePassiveSystem HonorPiecePassiveSystem => honorPiecePassiveSystem;
        public RegimeActionButtons RegimeActionButtons => regimeActionButtons;
        public AutonomousMovement AutonomousMovement => autonomousMovement;
        public StockfishBridge StockfishBridge => stockfishBridge;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            boardManager = SceneComponentResolver.Resolve(boardManager);
            turnManager = SceneComponentResolver.Resolve(turnManager);
            politicsManager = SceneComponentResolver.Resolve(politicsManager);
            militaryManager = SceneComponentResolver.Resolve(militaryManager);
            politicalManager = SceneComponentResolver.Resolve(politicalManager);
            rebellionSystem = SceneComponentResolver.Resolve(rebellionSystem);
            kingStateEffectApplier = SceneComponentResolver.Resolve(kingStateEffectApplier);
            honorPiecePassiveSystem = SceneComponentResolver.ResolveOrAdd(honorPiecePassiveSystem, gameObject);
            regimeActionButtons = SceneComponentResolver.ResolveOrAdd(regimeActionButtons, gameObject);
            autonomousMovement = SceneComponentResolver.Resolve(autonomousMovement);
            stockfishBridge = SceneComponentResolver.Resolve(stockfishBridge);

            if (politicsManager != null)
            {
                politicsManager.Initialize();
            }

            if (kingStateEffectApplier != null)
            {
                kingStateEffectApplier.Initialize();
            }

            if (honorPiecePassiveSystem != null)
            {
                honorPiecePassiveSystem.Initialize();
            }

            if (militaryManager != null)
            {
                militaryManager.Initialize();
            }

            if (politicalManager != null)
            {
                politicalManager.Initialize();
            }

            if (regimeActionButtons != null)
            {
                regimeActionButtons.Initialize();
            }

            if (rebellionSystem != null)
            {
                rebellionSystem.Initialize();
            }

            if (autonomousMovement != null)
            {
                autonomousMovement.Initialize();
            }

            if (stockfishBridge != null)
            {
                stockfishBridge.Initialize();
            }

            if (turnManager != null)
            {
                turnManager.StartGame();
            }
        }
    }
}
