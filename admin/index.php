<?php include '_header.php'; ?>
<div id="content">
	<?php
	$data = $hidenseq->active_downloads();

	table(array(
		'start_time' => 'Time',
	  	'path_info' => 'File',
	  	'chunk_size' => 'Size',
	  	'mime_type' => 'Type',
	  	'remote_addr' => 'IP',
	  	), $data, array(
	  	'chunk_size' => function ($value) {
	  		return sprintf("%.2fmb", $value / 1024 / 1024);
	  	}
	  	));
	?>
</div><!-- #content -->
<?php include '_footer.php'; ?>