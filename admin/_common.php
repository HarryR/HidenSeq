<?php
/*
 * Copyright (c) 2012 Gabriel Roberts. All Rights Reserved.
 */
$__db = NULL;
function db() {
	global $__db;
	if( $__db  ) return $__db;
	$__db = new PDO( 
	    'mysql:host=localhost;dbname=hidenseq', 
	    'hidenseq', 
	    'hidenseq', 
	    array(PDO::MYSQL_ATTR_INIT_COMMAND => "SET NAMES utf8",
	    	  PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC) 
	); 
	return $__db;
}

function param($name, $default = NULL) {
	if( isset($_REQUEST[$name]) ) {
		return $_REQUEST[$name];
	}
	return $default;
}

class HidenSeq {
	function active_downloads() {
		$db = db();
		$stmt = $db->query("SELECT * FROM downloads WHERE is_finished = 0");
		return $stmt->fetchAll(PDO::FETCH_ASSOC);
	}

	function finished_downloads($page = 0, $limit = 20) {
		$db = db();
		$stmt = $db->query(sprintf("SELECT * FROM downloads WHERE is_finished = 1 ORDER BY start_time DESC LIMIT %d, %d", $page * $limit, $limit));
		return $stmt->fetchAll(PDO::FETCH_ASSOC);
	}

	function chunks($download_id) {
		$db = db();
		$stmt = $db->prepare("SELECT * FROM download_chunks WHERE download_id = ?");
		$stmt->execute(array($download_id));
		return $stmt->fetchAll(PDO::FETCH_ASSOC);
	}

	function find_chunk($offset, $chunk_hash) {
		$db = db();
		$stmt = $db->prepare("SELECT download_id FROM download_chunks WHERE offset = ? AND hash_str = ?");
		$stmt->execute(array($offset, $chunk_hash));
		return $stmt->fetch(PDO::FETCH_ASSOC);
	}
}