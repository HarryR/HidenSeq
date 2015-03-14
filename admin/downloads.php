<?php include '_header.php'; ?>
<div id="content">
	<?php
	$data = $hidenseq->finished_downloads(intval(param('page')));

	table(array(
		'start_time' => 'Time',
	  	'path_info' => 'File',
	  	'chunk_size' => 'Size',
	  	'finish_time' => 'Speed',
	  	'mime_type' => 'Type',
	  	'remote_addr' => 'IP',
	  	), $data, array(
	  	'chunk_size' => function ($value) {
	  		return sprintf("%.2fmb", $value / 1024 / 1024);
	  	},
	  	'finish_time' => function ($value, $key, $row) {
	  		if( $value == NULL || $row['is_finished'] == 0 ) return 'unknown!';
	  		$finish = strtotime($value);
	  		$start = strtotime($row['start_time']);
	  		$speed = $row['chunk_size'] / ($finish - $start);
	  		return sprintf("%.2fmb/s", $speed / 1024 / 1024 );
	  	}
	  	));

	paginate('page', count($data) < 20);
	?>
</div><!-- #content -->
<?php include '_footer.php'; ?>