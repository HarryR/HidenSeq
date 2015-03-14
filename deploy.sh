#
# HidenSeq CDN Protection
# Debian 6 server deployment
#

###########################
# Install dependencies
#
sudo apt-get install nginx supervisor git mysql-server python-mysqldb python-pip python-dev
sudo pip install bottle eventlet
sudo update-rc.d supervisor defaults
sudo update-rc.d supervisor enable
sudo update-rc.d nginx enable
sudo update-rc.d mysql enable

DEPLOYMENT=`dirname $0`


###########################
# Setup deployment directory
#
mkdir -p $DEPLOYMENT/conf
cat <<<EOF > $DEPLOYMENT/run.sh
#!/bin/bash
BASEDIR=`dirname $0`
cd $BASEDIR
exec python current/proxy.py -p $1 -c $BASEDIR/conf/proxy.cfg

EOF
chmod 755 $DEPLOYMENT/run.sh


###########################
# Checkout and setup Proxy
#
git clone git@github.com:HarryR/HidenSeq.git $DEPLOYMENT/current
cat <<<EOF > $DEPLOYMENT/conf/proxy.cfg
[*listen]
host=0.0.0.0
port=8080
server=eventlet
debug=False

[*database]
host=localhost
port=3306
username=hidenseq
password=hidenseq
schema=hidenseq

[c201.example.com]
url=http://cdn1.example.com/
rxremove=&identcode=.*$
secret=lol123
speed_limit=253600

EOF

cat <<<EOF > /etc/supervisor/conf.d/hidenseq.conf
[program:proxy]
command=$DEPLOYMENT/run.sh 80%(process_num)02d
numprocs=5
process_name=port_80%(process_num)02d
directory=$DEPLOYMENT
user=harryr
autostart=true
autorestart=true
startsecs=10
startretries=30
stopwaitsecs=30
stdout_logfile_backups=2
EOF


###########################
# Configure NGINX
#
cat <<<EOF > /etc/nginx/nginx.conf
user www-data;
worker_processes  1;

error_log  /var/log/nginx/error.log;
pid        /var/run/nginx.pid;

events {
    worker_connections  1024;
}

http {
    include       /etc/nginx/mime.types;

    access_log  /var/log/nginx/access.log;

    sendfile        on;

    keepalive_timeout  65;
    tcp_nodelay        on;

    gzip  off;

    include /etc/nginx/conf.d/*.conf;
    include /etc/nginx/sites-enabled/*;
}
EOF

cat <<<EOF > /etc/nginx/sites-enabled/default
upstream backend {
	ip_hash;
        server localhost:8000;
        server localhost:8001;
        server localhost:8002;
        server localhost:8003;
        server localhost:8004;
}

limit_conn_zone $binary_remote_addr zone=addr:10m;

server {
        listen   [::]:80 default; 
        server_name  cdn.example.com;

        limit_rate 400k;
        limit_conn addr 2;

        location / {
                proxy_pass http://backend;
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
                proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                proxy_redirect off;
                proxy_buffering on;
                proxy_max_temp_file_size 0;
        }
}
EOF


###########################
# Restart services
#
sudo service supervisor restart
sudo service nginx restart
