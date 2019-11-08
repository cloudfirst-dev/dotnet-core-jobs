using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using k8s;
using k8s.Models;
using System.Collections;
using web.Models;

namespace web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IList<BatchJob>> Get()
        {
            // get the namespace the pod is running in
            // this is set with a FieldRef as part of the deployment
            var pod_namespace = Environment.GetEnvironmentVariable("MY_POD_NAMESPACE");

            // get the kube client
            var kube = GetKubernetes();

            var jobs = kube.ListNamespacedJob(pod_namespace);
            var output = new List<BatchJob>();

            // map output
            foreach (var job in jobs.Items) {
                string value;
                
                output.Add(new BatchJob {
                    Name = job.Metadata.Name,
                    Task = job.Metadata.Labels.TryGetValue("task", out value) ? value : null,
                    Status = job.Status
                });
            }

            return output;
        }

        // GET api/values/{id}
        [HttpGet]
        [Route("{name}")]
        public ActionResult<BatchJob> GetById(string name)
        {
            // get the namespace the pod is running in
            // this is set with a FieldRef as part of the deployment
            var pod_namespace = Environment.GetEnvironmentVariable("MY_POD_NAMESPACE");

            // get the kube client
            var kube = GetKubernetes();

            var job = kube.ReadNamespacedJob(name, pod_namespace);
            var output = new List<BatchJob>();

            // map output
            string value;
            return new BatchJob {
                Name = job.Metadata.Name,
                Task = job.Metadata.Labels.TryGetValue("task", out value) ? value : null,
                Status = job.Status
            };
        }

        // POST api/jobs
        [HttpPost]
        public ActionResult<String> Post()
        {
            // get the namespace the pod is running in
            // this is set with a FieldRef as part of the deployment
            var pod_namespace = Environment.GetEnvironmentVariable("MY_POD_NAMESPACE");
            var task_image = Environment.GetEnvironmentVariable("RUNNER_IMAGE");

            // get the kube client
            var kube = GetKubernetes();

            // create the job definition
            var job = new V1Job
            {
                Metadata = new V1ObjectMeta {
                    Name = "test-" + Guid.NewGuid()
                },
                Spec = new V1JobSpec {
                    Template = new V1PodTemplateSpec {
                        Metadata = new V1ObjectMeta {
                            Name = "runner",
                            Labels = new Dictionary<string,string> {
                                {"task", "test-task"}
                            }
                        },
                        Spec = new V1PodSpec {
                            Containers = new List<V1Container> {
                                new V1Container {
                                    Name = "runner",
                                    Image = task_image,
                                }
                            },
                            RestartPolicy = "Never"
                        }
                    }
                }
            };
            
            Console.WriteLine(job);

            // schedule the job
            var result = kube.CreateNamespacedJob(job, pod_namespace);

            return job.Metadata.Name;
        }

        private Kubernetes GetKubernetes() {
            KubernetesClientConfiguration config;
            
            // determine where to load config from
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"))) {
                config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            } else {
                config = KubernetesClientConfiguration.InClusterConfig();
            }
            
            // setup the kubernetes client
            return new Kubernetes(config);
        }
    }
}