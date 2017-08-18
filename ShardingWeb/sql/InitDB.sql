drop table if exists log;
CREATE TABLE `log` (
          `id` int(11) NOT NULL AUTO_INCREMENT,
          `date` datetime DEFAULT NULL,
          `thread` varchar(45) DEFAULT NULL,
          `level` varchar(45) DEFAULT NULL,
          `logger` varchar(45) DEFAULT NULL,
          `exception` varchar(45) DEFAULT NULL,
          `message` varchar(4000) DEFAULT NULL,
          `userid` varchar(45) DEFAULT NULL,
		  `enable` int(11),
          PRIMARY KEY (`id`)
        ) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=utf8;
alter table log comment '错误日志';
