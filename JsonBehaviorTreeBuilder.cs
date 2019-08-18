using Newtonsoft.Json;

namespace BetterDriver
{
    public class JsonBehaviorTreeBuilder : BehaviorTreeBuilder<JsonBehaviorTreeBuilder>
    {
        protected override JsonBehaviorTreeBuilder BuilderInstance => this;
        protected string content;

        public JsonBehaviorTreeBuilder(string json) => content = json;

        public override BehaviorTree Build()
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new MyContractResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore
            };
            result = JsonConvert.DeserializeObject<BehaviorTree>(content, settings);
            return base.Build();
        }
    }
}
