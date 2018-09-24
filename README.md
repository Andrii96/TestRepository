# Telelingua_ReportingServices_BackEnd

How to publish docker image:
1. From base folder call:
```powershell
docker build -t reporting -f .Dockerfile .
```
2. To start image locally call:
```powershell
docker run  -p 8080:80 --name myapp reporting -u
```
and open localhost:8080
3. To push image to repo call:
```docker push <repo-address>
```
4. To create .tar archive from docker image do:
```powershell
docker save -o <path-to-file>
```
5. To restore docker image from .tar file do:
```powershell
docker load -i <pth to file>
```
