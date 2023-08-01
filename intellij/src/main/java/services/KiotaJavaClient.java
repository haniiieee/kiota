package services;
import java.io.IOException;
import com.github.arteam.simplejsonrpc.client.JsonRpcClient;
import com.github.arteam.simplejsonrpc.client.Transport;
import com.github.arteam.simplejsonrpc.client.builder.RequestBuilder;

public class KiotaJavaClient{
    private final JsonRpcClient client;

    public KiotaJavaClient() {
        client = new JsonRpcClient(new ProcessTransport("kiota", "rpc"));
    }

    // Method to send a request and get the response
    public RequestBuilder<String> createRequest(String method, Class<String> returnType) {
        return client.createRequest()
                .method(method)
                .returnAs(returnType);
    }

}