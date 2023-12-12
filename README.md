## 啟動專案步驟

### 1. 啟動 Docker Prometheus 和 Jaeger
執行以下指令以啟動 Docker 容器：
```sh
docker-compose up -d
```

### 2. 啟動各個服務
分別啟動 Order、Payment 和 ShoppingCart 服務：

- **訂單服務 (Order Service)**
  ```sh
  dotnet run --urls="http://localhost:50051"
  ```

- **支付服務 (Payment Service)**
  ```sh
  dotnet run --urls="http://localhost:50052"
  ```

- **購物車服務 (Shopping Cart Service)**
  ```sh
  dotnet run --urls="http://localhost:50050"
  ```

> 可以直接使用dotnet run啟動，appsetting.json和launchSettings.json都有為這三個服務已經設定為


```
  "Services":{
    "ShoppingCartSrv": "http://localhost:50051",
    "PaymentSrv": "http://localhost:50052",
    "OrderSrv": "http://localhost:50053"
  }
```

### 其他資訊

- **Jeager UI**:
  - http://localhost:16686/

- **Prometheus UI**:
  - http://localhost:9090/
  - 查詢語法範例
    -  `{__name__=~".+"}`
    - `add_to_cart_total`

### DEMO

![Prometheus Test Endpoint 1](https://github.com/iewihc/will-ec-demo/assets/53833171/af75a280-f747-423c-85e4-cca911f97d30)

![Prometheus Test Endpoint 2](https://github.com/iewihc/will-ec-demo/assets/53833171/44caf37e-29c1-4c4b-aafe-97902fea1ac6)

