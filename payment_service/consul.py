import requests
import socket

def register_service_with_consul(service_name: str, service_port: int):
    host_ip = socket.gethostbyname(socket.gethostname())
    payload = {
        "Name": service_name,
        "ID": service_name,  # Unique ID
        "Address": host_ip,
        "Port": service_port,
        "Check": {
            "HTTP": f"http://{host_ip}:{service_port}/health",
            "Interval": "10s",
            "Timeout": "1s"
        }
    }

    try:
        requests.put("http://consul:8500/v1/agent/service/register", json=payload)
        print(f"{service_name} registered with Consul")
    except Exception as e:
        print(f"Failed to register {service_name} with Consul: {e}")

def discover_service(service_name: str):
    resp = requests.get(f"http://consul:8500/v1/catalog/service/{service_name}")
    if resp.ok:
        service = resp.json()[0] 
        return f"http://{service['ServiceAddress']}:{service['ServicePort']}"
    return None