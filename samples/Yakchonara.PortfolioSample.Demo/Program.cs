using Yakchonara.PortfolioSample.Common;
using Yakchonara.PortfolioSample.Elemental;
using Yakchonara.PortfolioSample.Facility;
using Yakchonara.PortfolioSample.SceneFlow;
using Yakchonara.PortfolioSample.UI;

static void Print(string label, object value)
{
    Console.WriteLine($"[{label}] {value}");
}

var sceneFlow = new SceneFlowCoordinator();
sceneFlow.SceneLoaded += scene => Print("scene", $"{scene} loaded");
sceneFlow.SceneUnloaded += scene => Print("scene", $"{scene} unloaded");

var bath = new BathFacility(
    id: 101,
    name: "BathFacility-A",
    capacity: 2,
    lineupCapacity: 3,
    temperature: new BathTemperatureController(currentTemperature: 36, targetTemperature: 40, tolerance: 2));

var sauna = new SaunaFacility(
    id: 201,
    name: "SaunaFacility-A",
    capacity: 1,
    lineupCapacity: 2);

var bathContext = new AdditiveSceneContext("Bath_House");
bathContext.RegisterFacility(bath);
bathContext.RegisterFacility(sauna);
sceneFlow.RegisterContext(bathContext);
Print("context", bathContext.Describe());

var bathBinder = new FacilityStatusBinder();
bathBinder.Bind(bath);
bath.StateChanged += snapshot => Print("bath-state", snapshot);

SceneTransitionResult enterResult = sceneFlow.EnterScene(
    new SceneTransitionRequest("Bath_House", new GridPosition(4, 2), "player-entered-facility"));
Print("enter", enterResult.Message);

FacilityAssignmentResult queueResult = bath.EnqueueHerb(new HerbRequest("Herb-001", "warm-bath"));
Print("queue", queueResult.Message);

FacilityAssignmentResult assignResult = bath.TryAssignNextHerb();
Print("assign", assignResult.Message);
Print("ui", bathBinder.CurrentView!);

var actionController = new ElementalActionController();
var fireElemental = new ElementalAgent("Elemental-Fire-01", ElementalType.Fire, defaultScale: 1.0);
actionController.Capture(fireElemental);
bool elementalAssigned = actionController.AssignToFacility(fireElemental, bath);
Print("elemental", $"assigned={elementalAssigned}, status={fireElemental.Status}, scale={fireElemental.CurrentScale:0.00}");
Print("ui", bathBinder.CurrentView!);

sauna.EnqueueHerb(new HerbRequest("Herb-002", "sauna"));
sauna.TryAssignNextHerb();
var saunaElemental = new ElementalAgent("Elemental-Fire-03", ElementalType.Fire);
actionController.Capture(saunaElemental);
actionController.AssignToFacility(saunaElemental, sauna);
Print("sauna", sauna.GetSnapshot());

bath.CompleteBath("Herb-001");
Print("complete", bath.GetSnapshot());

SceneTransitionResult exitResult = sceneFlow.ExitCurrentScene("player-returned-town");
Print("exit", exitResult.Message);
Print("context", bathContext.Describe());
