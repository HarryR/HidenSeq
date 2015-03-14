CREATE TABLE `downloads` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `path_info` varchar(255) NOT NULL,
  `remote_addr` varchar(45) NOT NULL,
  `user_agent` varchar(255) NOT NULL,
  `server_addr` varchar(45) NOT NULL,
  `is_finished` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `mime_type` varchar(45) NOT NULL,
  `start_time` datetime NOT NULL,
  `finish_time` datetime DEFAULT NULL,
  `file_offset` bigint(20) NOT NULL,
  `chunk_size` bigint(20) NOT NULL,
  `file_size` bigint(20) NOT NULL,
  `query_string` TEXT,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `download_chunks` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `download_id` bigint(20) unsigned NOT NULL,
  `offset` bigint(20) unsigned NOT NULL,
  `hash_str` char(40) NOT NULL,
  `stamp` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;