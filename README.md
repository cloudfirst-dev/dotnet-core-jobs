## Prequesites



## Automated Deployment

This quickstart can be deployed quickly using Ansible. Here are the steps.

1. Clone [this repo](https://github.com/cloudfirst-dev/dot-net-jobs)
2. `cd dot-net-jobs`
3. Run `ansible-galaxy install -r requirements.yml --roles-path=galaxy`
2. Log into an OpenShift cluster, then run the following command.
```
$ ansible-playbook -i ./.applier/ galaxy/openshift-applier/playbooks/openshift-cluster-seed.yml
```
3. If you are running on an OpenShift 4.x cluster be sure to create a Jenkins instance in the dot-net-jobs-build namespace

At this point you should have two projects created (`dot-net-jobs-build`, and `dot-net-jobs-dev`) with a pipeline in the `-build` project, and our job runner app and job images
