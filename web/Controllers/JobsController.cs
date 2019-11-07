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
    public class JobsController : ControllerBase
    {
        // POST api/jobs
        [HttpPost]
        public String Post()
        {
            // get the namespace the pod is running in
            // this is set with a FieldRef as part of the deployment
            var pod_namespace = Environment.GetEnvironmentVariable("MY_POD_NAMESPACE");

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