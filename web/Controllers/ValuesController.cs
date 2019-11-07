using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using k8s;
using k8s.Models;
using System.Collections;

namespace web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            KubernetesClientConfiguration config;
            
            // determine where to load config from
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"))) {
                config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            } else {
                config = KubernetesClientConfiguration.InClusterConfig();
            }
             
            var client = new Kubernetes(config);
            var list = client.ListNamespacedPod("dot-net-jobs-dev");
            var pod = list.Items[0];
            var pods = new List<String>();

            foreach (var item in list.Items)
            {
                pods.Add(item.Metadata.Name);
            }

            var job = new V1Job
            {
                Metadata = new V1ObjectMeta {
                    Name = "test-" + Guid.NewGuid()
                },
                Spec = new V1JobSpec {
                    Template = new V1PodTemplateSpec {
                        Metadata = new V1ObjectMeta {
                            Name = "runner"
                        },
                        Spec = new V1PodSpec {
                            Containers = new List<V1Container> {
                                new V1Container {
                                    Name = "runner",
                                    Image = "centos:7",
                                    Command = new List<string> {
                                        "bin/bash",
                                        "-c",
                                        "for i in 9 8 7 6 5 4 3 2 1 ; do echo $i ; done"
                                    }
                                }
                            },
                            RestartPolicy = "Never"
                        }
                    }
                }
            };

            var result = client.CreateNamespacedJob(job, "dot-net-jobs-dev");

            Console.WriteLine(result);

            return pods.ToArray();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
