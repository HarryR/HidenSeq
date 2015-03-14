<?php
require_once '_common.php';
require_once '_ui.php';
$hidenseq = new HidenSeq();
?>
<html>
	<head>
		<meta charset="UTF-8" />
		<title>HidenSeq <?php if(isset($title)) echo ' - ' . $title; ?></title>
		<link rel="stylesheet" type="text/css" media="screen" href="style.css" />
	</head>

	<body>
		<div id="main">
			<div id="header">
				<h1>HidenSeq</h1>
				<div id="menu">
					<ul>
						<li><a href="index.php">Status</a></li>
						<li><a href="downloads.php">Downloads</a></li>
					</ul>
					<div class="clear"></div>
				</div><!-- #menu -->
			</div><!-- #header -->

