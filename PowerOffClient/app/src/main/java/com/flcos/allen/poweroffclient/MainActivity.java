package com.flcos.allen.poweroffclient;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.net.Socket;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;

import java.io.BufferedWriter;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.lang.String;

public class MainActivity extends AppCompatActivity {
    private Socket socket;
    private static final int SERVERPORT = 5050;
    private static final String SERVER_IP = "192.168.0.4";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);


    }

    public void onClick(View view) {
        EditText ipStr = (EditText) findViewById(R.id.IPText);
        TextView info = (TextView) findViewById(R.id.InfoText);
        String ip = ipStr.getText().toString().trim();
        info.setText("准备连接服务器...");
        while (true) {
            socket = null;
            try {
                socket = new Socket(SERVER_IP, SERVERPORT);
                DataInputStream input = new DataInputStream(socket.getInputStream());
                DataOutputStream out = new DataOutputStream(socket.getOutputStream());
                info.setText("发送命令给服务器");
                out.writeUTF("shutdown");
                String ret = input.readUTF();
                if ("OK".equals(ret)) {

                    info.setText("客户端将关闭连接");

                }
                out.close();
                input.close();
            }
            catch (IOException ioEx){
                System.out.println("客户端 finally 异常:" + ioEx.getMessage());
            }finally {
                if (socket != null) {
                    try {
                        socket.close();
                        System.out.println("socket is closed");
                    } catch (IOException e) {
                        socket = null;
                        System.out.println("客户端 finally 异常:" + e.getMessage());
                    }
                }
            }
        }


    }
}
