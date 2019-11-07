using Newtonsoft.Json;
using k8s.Models;

namespace web.Models {
    public class BatchJob
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "task")]
        public string Task { get; set; }

        public V1JobStatus Status { get; set; }
    }
}